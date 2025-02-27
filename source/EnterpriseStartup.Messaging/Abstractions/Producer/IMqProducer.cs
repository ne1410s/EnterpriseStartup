// <copyright file="IMqProducer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Messaging.Abstractions.Producer;

using System;

/// <summary>
/// Produces mq messages.
/// </summary>
public interface IMqProducer
{
    /// <summary>
    /// Gets a value indicating whether the consumer is connected.
    /// </summary>
    public bool IsConnected { get; }

    /// <summary>
    /// Gets the exchange name.
    /// </summary>
    public string ExchangeName { get; }
}

/// <summary>
/// Produces mq messages of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The message type.</typeparam>
public interface IMqProducer<in T> : IMqProducer
{
    /// <summary>
    /// Produces a message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The correlation id.</returns>
    public Guid Produce(T message);
}