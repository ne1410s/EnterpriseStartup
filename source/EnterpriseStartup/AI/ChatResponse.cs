// <copyright file="ChatResponse.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.AI;

/// <summary>
/// A typed response from a chat completion client.
/// </summary>
/// <typeparam name="T">The response type.</typeparam>
public class ChatResponse<T>
{
    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    public T Message { get; set; } = default!;

    /// <summary>
    /// Gets or sets the usage.
    /// </summary>
    public Usage Usage { get; set; } = new(0);
}

/// <summary>
/// The token usage incurred as a result of the call.
/// </summary>
public class Usage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Usage"/> class.
    /// </summary>
    /// <param name="inputTokens">The input tokens.</param>
    /// <param name="outputTokens">The output tokens.</param>
    public Usage(int inputTokens, int outputTokens = 0)
    {
        this.InputTokens = inputTokens;
        this.OutputTokens = outputTokens;
    }

    /// <summary>
    /// Gets the input tokens.
    /// </summary>
    public int InputTokens { get; }

    /// <summary>
    /// Gets the output tokens.
    /// </summary>
    public int OutputTokens { get; }

    /// <summary>
    /// Gets the total tokens consumed.
    /// </summary>
    public int Tokens => this.InputTokens + this.OutputTokens;
}