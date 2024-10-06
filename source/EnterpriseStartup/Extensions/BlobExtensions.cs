// <copyright file="BlobExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Extensions;

using System;
using Azure.Storage.Blobs;
using EnterpriseStartup.Blobs.Abstractions;
using EnterpriseStartup.Blobs.AzureBlob;
using FluentErrors.Extensions;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions related to blobs.
/// </summary>
public static class BlobExtensions
{
    private const string Ephemeral = IUserBlobRepository.Ephemeral;
    private const string Permanent = IUserBlobRepository.Permanent;

    /// <summary>
    /// Adds the enterprise blobs feature. The config section must contain two
    /// Azure Storage connection strings under the keys Ephemeral and Permanent.
    /// These may be the same value.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="configSection">The name of the special config section.</param>
    /// <returns>The original parameter, for chainable commands.</returns>
    public static IServiceCollection AddEnterpriseUserBlobs(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSection = "AzureClients")
    {
        configuration.MustExist();
        var ephemeral = configuration[$"{configSection}:{Ephemeral}"];
        var permanent = configuration[$"{configSection}:{Permanent}"];
        services.AddAzureClients(o =>
        {
            o.AddBlobServiceClient(ephemeral).WithName(Ephemeral);
            o.AddBlobServiceClient(permanent).WithName(Permanent);
        });

        var timeout = TimeSpan.FromSeconds(5);
        services.AddScoped<IUserBlobRepository, AzureBlobRepository>()
            .AddHealthChecks()
            .AddAzureBlobStorage(sp => ResolveClient(sp, true), name: "azure_blob_ephemeral", timeout: timeout)
            .AddAzureBlobStorage(sp => ResolveClient(sp, false), name: "azure_blob_permanent", timeout: timeout);

        return services;
    }

    private static BlobServiceClient ResolveClient(IServiceProvider sp, bool ephemeral)
    {
        var factory = sp.GetRequiredService<IAzureClientFactory<BlobServiceClient>>();
        return ephemeral
            ? factory.CreateClient(IUserBlobRepository.Ephemeral)
            : factory.CreateClient(IUserBlobRepository.Permanent);
    }
}
