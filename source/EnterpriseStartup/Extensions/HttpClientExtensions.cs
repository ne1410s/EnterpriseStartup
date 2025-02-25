// <copyright file="HttpClientExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Extensions;

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

/// <summary>
/// Extensions for configuring HTTP clients.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Configures HTTP client to retry via a jittered backoff. 5xx and 408 HTTP
    /// response codes are covered. Use the <paramref name="alsoApplyTo"/> parameter
    /// to supply additional codes. (If null, this will supply 404s and 429s).
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="maxRetries">The maximum number of retries.</param>
    /// <param name="initialDelaySeconds">The initial median delay seconds.</param>
    /// <param name="alsoApplyTo">Other response codes. If null is passed, this
    /// list includes 404 and 429 (added to the already-present 5xx and 408).</param>
    /// <returns>The builder, for chainable calls.</returns>
    public static IHttpClientBuilder WithBackoff(
        this IHttpClientBuilder builder,
        int maxRetries = 5,
        double initialDelaySeconds = 5,
        IList<int>? alsoApplyTo = null)
    {
        return builder.AddPolicyHandler((_) =>
        {
            var initialDelay = TimeSpan.FromSeconds(initialDelaySeconds);
            var jitteredExponentialBackoff = Backoff.DecorrelatedJitterBackoffV2(initialDelay, maxRetries);
            alsoApplyTo ??= [404, 429];
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => alsoApplyTo.Contains((int)msg.StatusCode))
                .WaitAndRetryAsync(jitteredExponentialBackoff);
        });
    }
}
