// <copyright file="TestObjects.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;

public class FakeHttpClient(
    ResponseSignature? defaultResponse = null,
    Func<RequestSignature, ResponseSignature?>? behaviour = null) : HttpClient
{
    public IList<KeyValuePair<RequestSignature, ResponseSignature>> Calls { get; } = [];

    [ExcludeFromCodeCoverage]
    public override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var requestSignature = new RequestSignature(
            request.RequestUri!.ToString(),
            request.Method.ToString().ToUpper(CultureInfo.InvariantCulture),
            await request.Content!.ReadAsStringAsync(cancellationToken)!);

        var responseSignature = behaviour?.Invoke(requestSignature) ?? defaultResponse ?? new(200);
        this.Calls.Add(new(requestSignature, responseSignature));

        var response = new HttpResponseMessage((HttpStatusCode)responseSignature.Status);
        response.Content = new StringContent(responseSignature.Body!);

        return response;
    }
}

public record RequestSignature(string Path, string Method, string? Body = null);

public record ResponseSignature(int Status, string? Body = null);
