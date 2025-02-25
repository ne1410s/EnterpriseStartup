// <copyright file="OpenAIClientTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests.AI;

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;
using EnterpriseStartup.AI;
using FluentErrors.Errors;
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
    public async Task CompleteChat_ErrorResponse_ThrowsExpected()
    {
        // Arrange
        var userContent = new { hello = string.Empty };
        var client = GetSut(out _, _ => new(500, "{\"fale:\",true}"));

        // Act
        var act = client.CompleteChat<object>("1", userContent);

        // Assert
        await act.ShouldThrowAsync<HttpResponseException>();
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
        var result = await client.CompleteChat<object>("1", userContent, CancellationToken.None);

        // Assert
        result.Usage.Tokens.ShouldBeGreaterThan(0);
        fakeHttpClient.Calls[0].Key.Body!.ShouldContain(expected);
        fakeHttpClient.Dispose();
    }

    [Fact]
    public async Task CompleteChat_MultilinePrompt_GetsReplaced()
    {
        // Arrange
        var userContent = new { hello = "\"Mr\" Smith" };
        const string prompt = """
            hello
            there
            """;
        var client = GetSut(out var fakeHttpClient);

        // Act
        _ = await client.CompleteChat<ComplexResponse<string>>(prompt, userContent, CancellationToken.None);

        // Assert
        var request = fakeHttpClient.Calls[0].Key.Body!;
        request.ShouldContain("{ \"role\": \"system\", \"content\": \"hello there\" },");
        fakeHttpClient.Dispose();
    }

    [Fact]
    public async Task CompleteChat_ComplexResponseType_RemovesProblematicJson()
    {
        // Arrange
        var userContent = new { hello = "\"Mr\" Smith" };
        var client = GetSut(out var fakeHttpClient);

        // Act
        _ = await client.CompleteChat<ComplexResponse<string>>("1", userContent, CancellationToken.None);

        // Assert
        var sanitisedRequest = Regex.Replace(fakeHttpClient.Calls[0].Key.Body!, "\\s+", " ");
        sanitisedRequest.ShouldContain("\"anyOf\":");
        sanitisedRequest.ShouldContain("\"Email\": { \"type\": \"string\" },");
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

/// <summary>
/// Demo.
/// </summary>
/// <typeparam name="T">The type.</typeparam>
public class ComplexResponse<T>
{
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = default!;

    /// <summary>
    /// Gets or sets the exer.
    /// </summary>
    public T Exer { get; set; } = default!;
}
