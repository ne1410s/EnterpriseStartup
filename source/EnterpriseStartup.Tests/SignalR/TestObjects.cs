// <copyright file="TestObjects.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests.SignalR;

using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

public class FakeContext(ClaimsPrincipal? user, string connectionId = "connection") : HubCallerContext
{
    public int Aborts { get; private set; }

    public override string ConnectionId => connectionId;

    public override string? UserIdentifier { get; }

    public override ClaimsPrincipal? User => user;

    public override IDictionary<object, object?> Items { get; } = new Dictionary<object, object?>();

    public override IFeatureCollection Features { get; } = new FeatureCollection();

    public override CancellationToken ConnectionAborted { get; }

    public override void Abort() => this.Aborts++;
}
