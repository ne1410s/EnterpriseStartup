// <copyright file="OpenAIClientTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests.AI;

using System.Text.Json;
using System.Text.RegularExpressions;
using EnterpriseStartup.AI;
using Microsoft.Extensions.Options;

/// <summary>
/// Tests for the <see cref="OpenAIClient"/> class.
/// </summary>
public class OpenAIClientTests
{
    [Fact]
    public async Task CompleteChat_WhenCalled_CallsEndpoint()
    {
        // Arrange
        var client = GetSut(out var fakeHttpClient);

        // Act
        await client.CompleteChat<object>("1", new { });

        // Assert
        fakeHttpClient.Calls[0].Key.Path.ShouldStartWith("chat/completions?api-version=");
        fakeHttpClient.Dispose();
    }

    [Fact]
    public async Task CompleteChat_WhenCalled_SendsSystemPrompt()
    {
        // Arrange
        const string prompt = "Hello, Prompt!";
        const string expected = $$"""
            { "role": "system", "content": "{{prompt}}" }
            """;
        var client = GetSut(out var fakeHttpClient);

        // Act
        await client.CompleteChat<object>(prompt, new { });

        // Assert
        fakeHttpClient.Calls[0].Key.Body!.ShouldContain(expected);
        fakeHttpClient.Dispose();
    }

    [Fact]
    public async Task CompleteChat_EmptyStringInUserPayload_SendsExpectedContent()
    {
        // Arrange
        var userContent = new { hello = string.Empty };
        const string expected = """
            { "role": "user", "content": "{\"hello\":\"\"}" }
            """;
        var client = GetSut(out var fakeHttpClient);

        // Act
        await client.CompleteChat<object>("1", userContent);

        // Assert
        fakeHttpClient.Calls[0].Key.Body!.ShouldContain(expected);
        fakeHttpClient.Dispose();
    }

    [Fact]
    public async Task CompleteChat_QuotesInUserPayload_SendsExpectedContent()
    {
        // Arrange
        var userContent = new { hello = "\"Mr\" Smith" };
        const string expected = """
            { "role": "user", "content": "{\"hello\":\"\u0022Mr\u0022 Smith\"}" }
            """;
        var client = GetSut(out var fakeHttpClient);

        // Act
        await client.CompleteChat<object>("1", userContent, CancellationToken.None);

        // Assert
        fakeHttpClient.Calls[0].Key.Body!.ShouldContain(expected);
        fakeHttpClient.Dispose();
    }

    [Fact]
    public void ClientConfig_WhenCreated_HasExpected()
    {
        // Arrange & Act
        var config = new OpenAIClientConfig { BaseUrl = "1", ApiKey = "2" };

        // Assert
        config.BaseUrl.ShouldBe("1");
        config.ApiKey.ShouldBe("2");
    }

    [Fact]
    public void Usage_WhenCreated_HasExpected()
    {
        // Arrange & Act
        var usage = new Usage(1, 2);

        // Assert
        usage.Tokens.ShouldBe(3);
    }

    private static OpenAIClient GetSut(
        out FakeHttpClient fakeHttpClient,
        Func<RequestSignature, ResponseSignature?>? behaviour = null,
        OpenAIClientConfig? mockConfig = null)
    {
        var mockFactory = new Mock<IHttpClientFactory>();
        var mockConfigOpts = new Mock<IOptions<OpenAIClientConfig>>();
        mockConfigOpts.Setup(m => m.Value).Returns(mockConfig ?? new());

        fakeHttpClient = new FakeHttpClient(null, behaviour ?? GetDefaultBehaviour);
        mockFactory.Setup(m => m.CreateClient(It.IsAny<string>())).Returns(fakeHttpClient);

        return new OpenAIClient(mockFactory.Object, mockConfigOpts.Object);
    }

    private static ResponseSignature? GetDefaultBehaviour(RequestSignature requestSignature)
    {
        return new(200, GetFakeChatResponse());
    }

    private static string GetFakeChatResponse(object? innerContent = null)
    {
        innerContent ??= new { };
        var contentEscaped = Regex.Replace(JsonSerializer.Serialize(innerContent), @"([^\\])""", "$1\\\"");
        return $$"""
            {
              "choices": [ { "message": { "content": "{{contentEscaped}}" } } ],
              "usage": {
                "prompt_tokens": 1,
                "completion_tokens": 2
              }
            }
            """;
    }
}
