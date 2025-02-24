// <copyright file="ResponseContainer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.AI;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// A response container. Open AI / structured output does not support
/// deserialisation into list-types.
/// </summary>
/// <typeparam name="T">The response type, which may be any serialisable
/// type.</typeparam>
public class ResponseContainer<T>
{
    /// <summary>
    /// Gets or sets the response.
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    [Description("The response.")]
    [JsonPropertyName("response")]
    public T ResponseItem { get; set; } = default!;
}
