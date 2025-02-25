// <copyright file="IAIChatCompleter.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.AI;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Fulfils AI requests into structured output.
/// </summary>
public interface IAIChatCompleter
{
    /// <summary>
    /// Performs chat completion; returning a schema-bound object.
    /// </summary>
    /// <typeparam name="T">The structured return type.</typeparam>
    /// <param name="prompt">Instructions on how to process the input.</param>
    /// <param name="input">The input object.</param>
    /// <param name="cancel">A cancellation token.</param>
    /// <returns>A response object containing the schema-bound reply.</returns>
    Task<ChatResponse<T>> CompleteChat<T>(string prompt, object? input = null, CancellationToken cancel = default)
        where T : class;
}
