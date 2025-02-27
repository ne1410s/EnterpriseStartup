// <copyright file="MqProducerBase.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Messaging.Abstractions.Producer;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnterpriseStartup.Messaging.Abstractions.Consumer;

/// <inheritdoc cref="IMqProducer{T}"/>
public abstract class MqProducerBase<T> : IMqProducer<T>
{
    private readonly JsonSerializerOptions jsonOpts = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <summary>
    /// Fires when a message is about to be sent.
    /// </summary>
    public event EventHandler<MqEventArgs>? MessageSending;

    /// <summary>
    /// Fires when a message has been sent.
    /// </summary>
    public event EventHandler<MqEventArgs>? MessageSent;

    /// <inheritdoc/>
    public abstract bool IsConnected { get; }

    /// <summary>
    /// Gets the exchange name.
    /// </summary>
    public abstract string ExchangeName { get; }

    /// <inheritdoc/>
    public Guid Produce(T message)
    {
        var json = JsonSerializer.Serialize(message, this.jsonOpts);
        var args = new MqEventArgs { Message = json };
        this.MessageSending?.Invoke(this, args);
        var id = this.ProduceInternal(Encoding.UTF8.GetBytes(json), args.Headers);
        this.MessageSent?.Invoke(this, args);
        return id;
    }

    /// <summary>
    /// Produces the message internally.
    /// </summary>
    /// <param name="bytes">The message bytes.</param>
    /// <param name="headers">The initial headers.</param>
    /// <returns>The correlation id.</returns>
    protected internal abstract Guid ProduceInternal(byte[] bytes, Dictionary<string, object> headers);
}