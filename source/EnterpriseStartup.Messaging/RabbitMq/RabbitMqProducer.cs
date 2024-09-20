// <copyright file="RabbitMqProducer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Messaging.RabbitMq;

using System;
using System.Collections.Generic;
using EnterpriseStartup.Messaging.Abstractions.Producer;
using RabbitMQ.Client;

/// <inheritdoc cref="MqProducerBase{T}"/>
public abstract class RabbitMqProducer<T> : MqProducerBase<T>, IDisposable
{
    private const string DefaultRoute = "DEFAULT";

    private readonly IConnection connection;
    private readonly IModel channel;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqProducer{T}"/> class.
    /// </summary>
    /// <param name="connectionFactory">The connection factory.</param>
    protected RabbitMqProducer(IConnectionFactory connectionFactory)
    {
        this.connection = connectionFactory?.CreateConnection()
            ?? throw new ArgumentNullException(nameof(connectionFactory));
        this.channel = this.connection.CreateModel();
        this.channel.ExchangeDeclare(this.ExchangeName, ExchangeType.Direct, true);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        this.channel.Close();
        this.connection.Close();
        this.connection.Dispose();
    }

    /// <inheritdoc/>
    protected internal override void ProduceInternal(byte[] bytes, Dictionary<string, object> headers)
    {
        var props = this.channel.CreateBasicProperties();
        props.Headers = headers;
        props.Headers["x-attempt"] = 1L;
        props.Headers["x-born"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        props.Headers["x-guid"] = Guid.NewGuid().ToByteArray();

        this.channel.BasicPublish(this.ExchangeName, DefaultRoute, props, bytes);
    }
}
