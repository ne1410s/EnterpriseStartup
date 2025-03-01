// <copyright file="TraceThisAttribute.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Telemetry;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
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
    [ExcludeFromCodeCoverage]
    public override void OnExit(MethodExecutionArgs arg)
    {
        if (arg?.ReturnValue is Task task)
        {
            _ = task.ContinueWith(t => this.Dispose(), TaskScheduler.Current);
        }
        else
        {
            this.Dispose();
        }
    }

    /// <inheritdoc/>
    [DebuggerNonUserCode]
    public override void OnException(MethodExecutionArgs arg)
    {
        if (arg?.Exception != null)
        {
            var ex = arg.Exception;
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