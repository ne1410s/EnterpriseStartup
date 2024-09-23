// <copyright file="ConsumerHostingService.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Messaging.Abstractions.Consumer;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentErrors.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Hosted singleton that allows for scoped consumers.
/// </summary>
/// <typeparam name="TConsumer">The consumer type.</typeparam>
public sealed class ConsumerHostingService<TConsumer> : BackgroundService
    where TConsumer : MqConsumerBase
{
    private readonly ILogger logger;
    private readonly TConsumer consumer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsumerHostingService{TConsumer}"/> class.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public ConsumerHostingService(IServiceProvider provider, ILoggerFactory loggerFactory)
    {
        var consumerType = this.GetType().GetGenericArguments()[0];
        var loggerCategory = $"{nameof(ConsumerHostingService<TConsumer>)}<{consumerType.Name}>";
        this.logger = loggerFactory.MustExist().CreateLogger(loggerCategory)!;
        using var scope = provider.CreateScope();
        this.consumer = scope.ServiceProvider.GetRequiredService<TConsumer>();
    }

    /// <inheritdoc/>
    [SuppressMessage("S1", "S2190:Loops and recursions.", Justification = "Per design")]
    [SuppressMessage("S2", "S6667:Logging in catch clause.", Justification = "Per design")]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            if (!this.consumer.IsConnected)
            {
                this.logger.LogInformation("Starting up...");
                try
                {
                    await this.consumer.StartAsync(stoppingToken);
                    this.logger.LogInformation("Started ok!");
                }
                catch (Exception ex)
                {
                    this.logger.LogWarning("Failed to start: [{ExceptionName}]", ex.GetType().Name);
                }
            }

            await Task.Delay(10000, stoppingToken);
        }
    }
}
