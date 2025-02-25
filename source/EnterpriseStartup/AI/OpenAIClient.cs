// <copyright file="OpenAIClient.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.AI;

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentErrors.Extensions;
using Microsoft.Extensions.Options;
using NJsonSchema;

/// <summary>
/// Open AI client.
/// </summary>
/// <param name="clientFactory">An http client factory.</param>
/// <param name="configOpts">The config options.</param>
public class OpenAIClient(
    IHttpClientFactory clientFactory,
    IOptions<OpenAIClientConfig> configOpts) : IAIChatCompleter
{
    private const StringComparison CIComparison = StringComparison.OrdinalIgnoreCase;
    private static readonly Regex FormatRemover = new Regex(
        ",?\\s+\"format\": \"\\w+\"|\\s+\"format\": \"\\w+\",?", RegexOptions.Compiled);

    private readonly HttpClient httpClient = clientFactory.CreateClient(nameof(OpenAIClient));
    private readonly OpenAIClientConfig config = configOpts.Value;

    /// <inheritdoc/>
    public async Task<ChatResponse<T>> CompleteChat<T>(string prompt, object input, CancellationToken cancel = default)
        where T : class
    {
        return await this.CompleteChatInternal<T>(prompt, input, cancel);
    }

    private static ChatResponse<T> ParseChatResponse<T>(JsonNode node)
    {
        var message = node["choices"]![0]!["message"]!["content"]!.GetValue<string>();
        var pTokens = node["usage"]!["prompt_tokens"]!.GetValue<int>();
        var cTokens = node["usage"]!["completion_tokens"]!.GetValue<int>();
        return new ChatResponse<T>()
        {
            Message = JsonSerializer.Deserialize<ResponseContainer<T>>(message)!.ResponseItem,
            Usage = new Usage(pTokens, cTokens),
        };
    }

    private async Task<ChatResponse<T>> CompleteChatInternal<T>(
        string prompt, object input, CancellationToken cancel = default)
        where T : class
    {
        var userJsonEscaped = JsonSerializer.Serialize(input).Replace("\"", "\\\"", CIComparison);
        var requestJson = $$"""
        {
            "model": "{{this.config.Model}}",
            "messages": [
            { "role": "system", "content": "{{Regex.Replace(prompt, @"\r\n?|\n", " ")}}" },
            { "role": "system", "content": "Please take the user's request and return a JSON response." },
            { "role": "user", "content": "{{userJsonEscaped}}" }
            ],
            "response_format": {
            "type": "json_schema",
            "json_schema": {
                "strict": true,
                "name": "postResult",
                "description": "Posts the result",
                "schema": {{JsonSchema.FromType<ResponseContainer<T>>().ToJson()}}
            }
            }
        }
        """;

        // No "format" properties allowed by gpt :/
        requestJson = FormatRemover.Replace(requestJson, string.Empty);
        requestJson = requestJson.Replace("\"oneOf\":", "\"anyOf\":", CIComparison);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var uri = new Uri($"{this.config.ApiVersion}/chat/completions", UriKind.Relative);
        var response = await this.httpClient.PostAsync(uri, request, cancel);
        await response.MustBeOk();

        await using var stream = await response.Content.ReadAsStreamAsync(cancel);
        var node = await JsonNode.ParseAsync(stream, cancellationToken: cancel);
        return ParseChatResponse<T>(node!);
    }
}