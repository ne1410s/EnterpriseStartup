// <copyright file="ConsumerHostingService.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Messaging.Abstractions.Consumer;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Hosted singleton that allows for scoped consumers.
/// </summary>
/// <typeparam name="TConsumer">The consumer type.</typeparam>
public sealed class ConsumerHostingService<TConsumer> : IHostedService, IDisposable
    where TConsumer : MqConsumerBase
{
    private readonly IServiceScope originalScope;
    private readonly TConsumer consumer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsumerHostingService{TConsumer}"/> class.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public ConsumerHostingService(IServiceProvider services, ILoggerFactory loggerFactory)
    {
        var consumerType = this.GetType().GetGenericArguments()[0];
        var loggerCategory = $"{nameof(ConsumerHostingService<TConsumer>)}<{consumerType.Name}>";
        var logger = loggerFactory?.CreateLogger(loggerCategory)!;
        this.originalScope = services.CreateScope();
        var sp = this.originalScope.ServiceProvider;
        try
        {
            this.consumer = sp.GetRequiredService<TConsumer>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start consumer hosting service");
            this.consumer = null!;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        this.originalScope.Dispose();
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (this.consumer != null)
        {
            await this.consumer.StartAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken)
        => await this.consumer.StopAsync(cancellationToken);
}
