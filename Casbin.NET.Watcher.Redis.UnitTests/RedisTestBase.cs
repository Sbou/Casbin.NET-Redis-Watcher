using System;
using Moq;
using StackExchange.Redis;

namespace Casbin.NET.Watcher.Redis.UnitTests
{
    public class RedisTestBase
    {
        private Action<RedisChannel, RedisValue> redisCallback;

        protected IConnectionMultiplexer GetMockedConnection(SubscriptionType subscriptionType)
        {
            var mock = new Mock<IConnectionMultiplexer>(MockBehavior.Default);
            var subMock = new Mock<ISubscriber>(MockBehavior.Default);

            switch (subscriptionType)
            {
                case SubscriptionType.Subscriber:
                    mock.Setup(connection => connection.GetSubscriber(It.IsAny<object>()))
                        .Returns(() =>
                        {
                            var subMock = new Mock<ISubscriber>(MockBehavior.Default);
                            subMock.Setup(sub => sub.Subscribe(It.IsAny<RedisChannel>(), It.IsAny<Action<RedisChannel, RedisValue>>(), It.IsAny<CommandFlags>()))
                                    .Callback((RedisChannel channelName, Action<RedisChannel, RedisValue> callback, CommandFlags flags) => redisCallback = callback);
                            return subMock.Object;
                        });
                    break;
                case SubscriptionType.Publisher:
                    mock.Setup(connection => connection.GetSubscriber(It.IsAny<object>()))
                        .Returns(() =>
                        {
                            var subMock = new Mock<ISubscriber>(MockBehavior.Default);
                            subMock.Setup(sub => sub.PublishAsync(It.IsAny<RedisChannel>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
                                    .Callback((RedisChannel channelName, RedisValue message, CommandFlags flags) => redisCallback?.Invoke(channelName, message));
                            return subMock.Object;
                        });
                    break;
                case SubscriptionType.Both:
                    mock.SetupSequence(connection => connection.GetSubscriber(It.IsAny<object>()))
                        .Returns(() =>
                        {
                            var subMock = new Mock<ISubscriber>(MockBehavior.Default);
                            subMock.Setup(sub => sub.Subscribe(It.IsAny<RedisChannel>(), It.IsAny<Action<RedisChannel, RedisValue>>(), It.IsAny<CommandFlags>()))
                                    .Callback((RedisChannel channelName, Action<RedisChannel, RedisValue> callback, CommandFlags flags) => redisCallback = callback);
                            return subMock.Object;
                        })
                        .Returns(() =>
                        {
                            var subMock = new Mock<ISubscriber>(MockBehavior.Default);
                            subMock.Setup(sub => sub.PublishAsync(It.IsAny<RedisChannel>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
                                    .Callback((RedisChannel channelName, RedisValue message, CommandFlags flags) => redisCallback?.Invoke(channelName, message));
                            return subMock.Object;
                        });
                    break;
            }

            return mock.Object;
        }
    }
}
