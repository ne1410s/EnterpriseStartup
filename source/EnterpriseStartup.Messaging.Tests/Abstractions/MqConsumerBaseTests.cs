// <copyright file="MqConsumerBaseTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Messaging.Tests.Abstractions;

using EnterpriseStartup.Messaging.Abstractions.Consumer;

/// <summary>
/// Tests for the <see cref="MqConsumerBase{T}"/> class.
/// </summary>
public class MqConsumerBaseTests
{
    [Fact]
    public void Ctor_WhenCalled_SetsQueueName()
    {
        // Arrange
        const string expected = "pascal-case-basic-thing";

        // Act
        var sut = new GenericConsumer();

        // Assert
        sut.QueueName.ShouldBe(expected);
    }

    [Fact]
    public async Task ConsumeInternal_NullArgs_ThrowsException()
    {
        // Arrange
        var sut = new GenericConsumer();

        // Act
        var act = () => sut.TestConsume(new BasicPayload(null), null!);

        // Assert
        (await act.ShouldThrowAsync<ArgumentNullException>())
            .ParamName.ShouldBe("args");
    }

    [Fact]
    public async Task ConsumeInternal_UnimplementedEvents_CallsMessageReceived()
    {
        // Arrange
        var sut = new GenericConsumer();
        var count = 0;
        sut.MessageReceived += (_, _) => count++;

        // Act
        await sut.TestConsume(new BasicPayload(null), GetMqArgs());
        await sut.StartAsync(CancellationToken.None);
        await sut.StopAsync(CancellationToken.None);

        // Assert
        count.ShouldBe(1);
        sut.ExchangeName.ShouldBe(TestHelper.TestExchangeName);
    }

    [Fact]
    public async Task ConsumeInternal_LowerCaseJson_ParsesAsExpected()
    {
        // Arrange
        var sut = new GenericConsumer();
        bool? didRetry = null;
        sut.MessageFailed += (_, args) => didRetry = args.Retry;
        const string message = "{ \"permafail\": false }";

        // Act
        await sut.TestConsume(message, GetMqArgs(message: message));

        // Assert
        (didRetry ?? false).ShouldBeTrue();
    }

    [Fact]
    public async Task ConsumeInternal_MalformedJson_ThrowsExpected()
    {
        // Arrange
        var sut = new GenericConsumer();
        Exception? ex = null;
        sut.MessageFailed += (_, args) => ex = args.Error;
        const string json = "{ <>nope!\"";
        const string expectedError = "Failed to parse message.";

        // Act
        await sut.TestConsume(json, GetMqArgs(message: json));

        // Assert
        ex.ShouldBeOfType<PermanentFailureException>()
            .Message.ShouldBe(expectedError);
    }

    [Theory]
    [InlineData(1L, 5L, true)]
    [InlineData(1L, null, true)]
    [InlineData(5L, 5L, false)]
    public async Task ConsumeInternal_VaryingTempFailAttempts_RetryAsExpected(
        long attempt, long? maxAttempts, bool expectRetry)
    {
        // Arrange
        var sut = new GenericConsumer(maxAttempts);
        var failArgs = (MqFailedEventArgs)null!;
        sut.MessageFailed += (_, args) => failArgs = args;

        // Act
        await sut.TestConsume(new BasicPayload(false), GetMqArgs(attempt));

        // Assert
        failArgs.Retry.ShouldBe(expectRetry);
    }

    [Fact]
    public void DoRetry_NullArgs_ThrowsException()
    {
        // Arrange
        var sut = new GenericConsumer();

        // Act
        Action act = () => sut.TestDoRetry(null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>()
            .ParamName.ShouldBe("args");
    }

    [Fact]
    public async Task StartStopAsync_WhenCalled_CallsStartStopInternal()
    {
        // Arrange
        var sut = new GenericConsumer();
        var expected = new[] { "StartInternal", "StopInternal" };

        // Act
        await sut.StartAsync(CancellationToken.None);
        await sut.StopAsync(CancellationToken.None);

        // Assert
        sut.Lifecycle.ToArray().ShouldBeEquivalentTo(expected);
    }

    private static MqConsumerEventArgs GetMqArgs(long attempt = 1, string message = "hi")
    {
        return new()
        {
            AttemptNumber = attempt,
            BornOn = 1,
            DeliveryId = 1,
            Message = message,
            MessageGuid = Guid.NewGuid(),
        };
    }
}
