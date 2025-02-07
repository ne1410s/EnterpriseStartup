﻿// <copyright file="TelemeterTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests.Telemetry;

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using FluentErrors.Extensions;
using EnterpriseStartup.Telemetry;

/// <summary>
/// Tests for the <see cref="Telemeter"/> class.
/// </summary>
public class TelemeterTests
{
    [Theory]
    [InlineData("1.2.3", "1.2.3")]
    [InlineData("1.0.2.6", "1.0.2")]
    [InlineData(null, null)]
    public void Ctor_WhenCalled_SetsNameAndVersion(string? asmVersion, string? appVersion)
    {
        // Arrange
        var assemblyName = new AssemblyName("test")
        {
            Version = asmVersion == null ? null : new(asmVersion),
        };

        // Act
        using var sut = new Telemeter(assemblyName);

        // Assert
        sut.AppTracer.Name.ShouldBe(assemblyName.Name);
        sut.AppMeter.Name.ShouldBe(assemblyName.Name);
        sut.AppTracer.Version.ShouldBe(appVersion);
        sut.AppMeter.Version.ShouldBe(appVersion);
    }

    [Fact]
    public void Ctor_WhenCalled_SetsTags()
    {
        // Arrange
        ActivityTagsCollection expected = new()
        {
            ["namespace"] = "test namespace",
            ["app"] = "test app",
            ["pod"] = "test pod",
        };
        Environment.SetEnvironmentVariable("K8S_NAMESPACE", $"{expected["namespace"]}");
        Environment.SetEnvironmentVariable("K8S_APP", $"{expected["app"]}");
        Environment.SetEnvironmentVariable("K8S_POD", $"{expected["pod"]}");

        // Act
        using var sut = new Telemeter();

        // Assert
        sut.AppTags.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public void StartTrace_WithTags_IncludesTags()
    {
        // Arrange
        using var sut = new Telemeter();
        using var listener = GetListener();
        var tag = new KeyValuePair<string, object?>("foo", "bar");

        // Act
        using var activity = sut.StartTrace("foo", tags: tag);

        // Assert
        activity!.Tags.Any(t => t.Value == "bar").ShouldBeTrue();
    }

    [Fact]
    public void StartTrace_WithParentContext_IncludesTags()
    {
        // Arrange
        using var sut = new Telemeter();
        using var listener = GetListener();
        var tag = new KeyValuePair<string, object?>("foo", "bar");

        // Act
        using var activity = sut.StartTrace("foo", parentContext: default, tags: tag);

        // Assert
        activity!.Tags.Any(t => t.Value == "bar").ShouldBeTrue();
    }

    [Fact]
    public void CaptureMetric_UnrecognisedType_ThrowsException()
    {
        // Arrange
        using var sut = new Telemeter();

        // Act
        var act = () => sut.CaptureMetric((MetricType)54321, 0, "foobar");

        // Assert
        act.ShouldThrow<ArgumentException>().ShouldSatisfyAllConditions(
            ex => ex.Message.ShouldMatch("Unrecognised metric type: 54321.*"),
            ex => ex.ParamName.ShouldBe("metricType"));
    }

    [Theory]
    [InlineData(MetricType.Counter)]
    [InlineData(MetricType.CounterNegatable)]
    [InlineData(MetricType.Histogram)]
    public void CaptureMetric_ValidTypes_RelayTags(MetricType metricType)
    {
        // Arrange
        using var sut = new Telemeter();
        var tag = new KeyValuePair<string, object?>("foo", "bar");
        const string metricName = "foobar";
        const int testValue = 42;
        using var meterListener = new MeterListener();
        void OnCreate(Instrument i) => meterListener.EnableMeasurementEvents(i, i.Name);
        var handled = false;
        meterListener.SetMeasurementEventCallback<int>((instr, val, tags, name) =>
        {
            handled = true;
            name.ShouldBe(metricName);
            val.ShouldBe(testValue);
            tags.ToArray().Select(t => t.Value).OfType<string>().Any(t => t == "bar").ShouldBeTrue();
            instr.Name.ShouldBe(metricName);
        });
        meterListener.Start();

        // Act
        var instrument = sut.CaptureMetric(metricType, 42, metricName, onCreate: OnCreate, tags: tag);

        // Assert
        instrument.Name.ShouldBe(metricName);
        _ = handled.MustBe(true);
    }

    private static ActivityListener GetListener()
    {
        var activityListener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(activityListener);
        return activityListener;
    }
}
