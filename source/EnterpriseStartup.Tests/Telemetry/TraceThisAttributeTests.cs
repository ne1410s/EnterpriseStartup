// <copyright file="TraceThisAttributeTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests.Telemetry;

using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EnterpriseStartup.Telemetry;
using MethodBoundaryAspect.Fody.Attributes;

/// <summary>
/// Tests for the <see cref="TraceThisAttribute"/> class.
/// </summary>
public class TraceThisAttributeTests
{
    [Fact]
    public void Ctor_NoTelemeter_AutoCreates()
    {
        // Arrange & Act
        using var sut = new TraceThisAttribute();

        // Assert
        _ = sut.Telemeter.ShouldNotBeNull();
    }

    [Fact]
    public void OnEntry_NullArg_ThrowsException()
    {
        // Arrange
        using var sut = new TraceThisAttribute();

        // Act
        var act = () => sut.OnEntry(null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("arg");
    }

    [Fact]
    public void OnEntry_WithArg_SetsDisposedToFalse()
    {
        // Arrange
        using var activity = new Activity("test");
        using var sut = GetSut(out _, activity);

        // Act
        sut.OnEntry(GetArgs());

        // Assert
        (sut.IsDisposed ?? true).ShouldBeFalse();
    }

    [Fact]
    public void OnEntry_WithArg_WritesToTrace()
    {
        // Arrange
        using var activity = new Activity("test");
        using var sut = GetSut(out _, activity);
        using var ms = new MemoryStream();
        _ = Trace.Listeners.Add(new TextWriterTraceListener(ms));

        // Act
        sut.OnEntry(GetArgs());

        // Assert
        Trace.Flush();
        var text = Encoding.UTF8.GetString(ms.ToArray());
        text.ShouldContain(nameof(TraceThisAttributeTests));
    }

    [Fact]
    public void OnEntry_WithArg_StartsExpectedTrace()
    {
        // Arrange
        using var activity = new Activity("test");
        using var sut = GetSut(out var mockTelemeter, activity);
        const string expectedName = "[EnterpriseStartup.Tests] TraceThisAttributeTests::GetArgs()";
        const ActivityKind expectedKind = ActivityKind.Internal;

        // Act
        sut.OnEntry(GetArgs());

        // Assert
        mockTelemeter.Verify(m => m.StartTrace(expectedName, expectedKind, default));
    }

    [Fact]
    public void OnException_NullArg_DoesNotThrow()
    {
        // Arrange
        using var sut = GetSut(out _);

        // Act
        var act = () => sut.OnException(null!);

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void OnException_NoActivity_DoesNotThrow()
    {
        // Arrange
        using var sut = GetSut(out _);

        // Act
        var act = () => sut.OnException(GetErrorArgs());

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void OnException_WithInner_AddsEvent()
    {
        // Arrange
        using var activity = new Activity("test");
        using var sut = GetSut(out _, activity);
        var expectedTags = new Dictionary<string, object?>
        {
            ["type"] = "ArithmeticException",
            ["message"] = "randomage",
            ["innerType"] = "DivideByZeroException",
            ["innerMessage"] = "ra",
        };

        // Act
        sut.OnEntry(GetArgs());
        sut.OnException(GetErrorArgs());

        // Assert
        var evt = activity.Events.Single();
        evt.Name.ShouldBe("exception");
        evt.Tags.ToArray().ShouldBeEquivalentTo(expectedTags.ToArray());
    }

    [Fact]
    public void OnExit_NullArg_DoesNotThrow()
    {
        // Arrange
        using var sut = new TraceThisAttribute();

        // Act
        var act = () => sut.OnExit(null!);

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public async Task OnExit_SyncMethod_CallsDispose()
    {
        // Arrange
        using var sut = GetSut(out _);

        // Act
        sut.OnExit(new MethodExecutionArgs
        {
            Method = MethodBase.GetCurrentMethod(),
            ReturnValue = null,
        });
        await Task.Delay(100);

        // Assert
        (sut.IsDisposed ?? false).ShouldBeTrue();
    }

    [Fact]
    public async Task OnExit_SyncMethodWithReturnValue_CallsDispose()
    {
        // Arrange
        using var sut = GetSut(out _);
        var args = await GetArgsAsync();

        // Act
        sut.OnExit(args);
        await Task.Delay(100);

        // Assert
        (sut.IsDisposed ?? false).ShouldBeTrue();
    }

    [Fact]
    public async Task OnExit_AsyncMethod_CallsDispose()
    {
        // Arrange
        using var sut = GetSut(out _);

        // Act
        sut.OnExit(await GetArgsAsync());
        await Task.Delay(100);

        // Assert
        (sut.IsDisposed ?? false).ShouldBeTrue();
    }

    private static TraceThisAttribute GetSut(
        out Mock<ITelemeter> mockTelemeter,
        Activity? activity = null)
    {
        mockTelemeter = new Mock<ITelemeter>();
        _ = mockTelemeter
            .Setup(m => m.AppTracer)
            .Returns(activity?.Source!);
        _ = mockTelemeter
            .Setup(m => m.StartTrace(
                It.IsAny<string>(),
                It.IsAny<ActivityKind>(),
                It.IsAny<ActivityContext>(),
                It.IsAny<KeyValuePair<string, object?>[]>()))
            .Returns(activity);

        return new TraceThisAttribute { Telemeter = mockTelemeter.Object };
    }

    private static MethodExecutionArgs GetArgs(object? returnValue = null)
    {
        return new MethodExecutionArgs
        {
            Method = MethodBase.GetCurrentMethod(),
            ReturnValue = returnValue,
        };
    }

    private static async Task<MethodExecutionArgs> GetArgsAsync()
    {
        await Task.CompletedTask;
        return new MethodExecutionArgs
        {
            Method = MethodBase.GetCurrentMethod(),
            ReturnValue = Task.FromResult(3),
        };
    }

    private static MethodExecutionArgs GetErrorArgs()
    {
        return new MethodExecutionArgs
        {
            Method = MethodBase.GetCurrentMethod(),
            Exception = new ArithmeticException(
                "randomage",
                new DivideByZeroException("ra")),
        };
    }
}