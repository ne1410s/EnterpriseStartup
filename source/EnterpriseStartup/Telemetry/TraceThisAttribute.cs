﻿// <copyright file="TraceThisAttribute.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Telemetry;

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using FluentErrors.Extensions;
using MethodBoundaryAspect.Fody.Attributes;

/// <summary>
/// Attribute that automatically captures basic trace data.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TraceThisAttribute"/> class.
/// </remarks>
[AttributeUsage(AttributeTargets.All)]
public sealed class TraceThisAttribute : OnMethodBoundaryAspect, IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the object has been disposed.
    /// </summary>
    public bool? IsDisposed { get; private set; }

    /// <summary>
    /// Gets or sets the telemeter instance.
    /// </summary>
    public ITelemeter Telemeter { get; set; } = new Telemeter();

    /// <summary>
    /// Gets the activity.
    /// </summary>
    public Activity? Activity { get; private set; }

    /// <inheritdoc/>
    public override void OnEntry(MethodExecutionArgs arg)
    {
        var method = arg?.Method ?? throw new ArgumentNullException(nameof(arg));

        // exclude getters and setters and the like
        if (!method.IsSpecialName)
        {
            var activityName = GetActivityName(method);
            this.Activity = this.Telemeter.StartTrace(activityName);
            this.IsDisposed = false;
            Trace.TraceInformation($"Activity on {this.Telemeter.AppTracer.Name}; {activityName}");
        }
    }

    /// <inheritdoc/>
    public override void OnExit(MethodExecutionArgs arg)
    {
        arg.MustExist();
        if (arg.ReturnValue is Task task)
        {
            task.ContinueWith(_ => this.Dispose(), TaskScheduler.Default);
        }
        else
        {
            this.Dispose();
        }
    }

    /// <inheritdoc/>
    public override void OnException(MethodExecutionArgs arg)
    {
        var ex = arg?.Exception ?? throw new ArgumentNullException(nameof(arg));
        var tags = new ActivityTagsCollection
        {
            ["type"] = ex.GetType().Name,
            ["message"] = ex.Message,
        };

        if (ex.InnerException != null)
        {
            tags.Add("innerType", ex.InnerException.GetType().Name);
            tags.Add("innerMessage", ex.InnerException.Message);
        }

        this.Activity?.AddEvent(new("exception", tags: tags));
        this.Dispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.IsDisposed = true;
        this.Activity?.Dispose();
    }

    private static string GetActivityName(MethodBase method)
    {
        var declaringAssembly = Assembly.GetAssembly(method.DeclaringType!);
        var prefix = declaringAssembly!.GetName().Name;
        return $"[{prefix}] {method.DeclaringType!.Name}::{method.Name}()";
    }
}