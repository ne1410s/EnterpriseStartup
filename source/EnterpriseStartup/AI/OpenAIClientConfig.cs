// <copyright file="OpenAIClientConfig.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.AI;

/// <summary>
/// Configuration for the <see cref="OpenAIClient"/>.
/// </summary>
public class OpenAIClientConfig
{
    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string Model { get; set; } = default!;

    /// <summary>
    /// Gets or sets the base url.
    /// </summary>
    public string BaseUrl { get; set; } = default!;

    /// <summary>
    /// Gets or sets the api key.
    /// </summary>
    public string ApiKey { get; set; } = default!;

    /// <summary>
    /// Gets or sets the api version.
    /// </summary>
    public string ApiVersion { get; set; } = default!;

    /// <summary>
    /// Gets or sets the http client timeout.
    /// </summary>
    public int HttpTimeoutSeconds { get; set; } = 30;
}