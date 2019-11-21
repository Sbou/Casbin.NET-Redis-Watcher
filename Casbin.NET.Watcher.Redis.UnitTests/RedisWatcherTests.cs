using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Redis.Casbin.NET;
using StackExchange.Redis;

namespace Casbin.NET.Watcher.Redis.UnitTests
{
    [TestClass]
    public class RedisWatcherTests
    {
        private Action<RedisChannel, RedisValue> redisCallback;

        public IConnectionMultiplexer RedisConnection { get; private set; }

        private IConnectionMultiplexer GetMockedConnection()
        {
            var mock = new Mock<IConnectionMultiplexer>(MockBehavior.Default);
            var subMock = new Mock<ISubscriber>(MockBehavior.Default);

            // The first time it returns a subscriber and the second one a publish
            // It's to be able to use the callback set in the subscriber in the publisher
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

            return mock.Object;
        }

        [TestInitialize]
        public void Initialize()
        {
#if TRUEREDIS
            RedisConnection = ConnectionMultiplexer.Connect("localhost:6379");
#else
            RedisConnection = GetMockedConnection();
#endif
        }

        [TestMethod]
        public void BadConnectionStringTest()
        {
            var connectionString = "unkonwnhost:6379";

            Assert.ThrowsException<StackExchange.Redis.RedisConnectionException>(() => new RedisWatcher(connectionString));
        }

        [TestMethod]
        public void NominalTest()
        {
            var callback = new TaskCompletionSource<int>();
            
            var watcher = new RedisWatcher(RedisConnection);
            watcher.SetUpdateCallback(() => callback.TrySetResult(1));

            var watcher2 = new RedisWatcher(RedisConnection);
            watcher2.Update();

            Assert.IsTrue(callback.Task.Wait(300), "The first watcher didn't receive the notification");
        }

        [TestMethod]
        public void IgnoreSelfMessageTest()
        {
            var callback = new TaskCompletionSource<int>();

            var watcher = new RedisWatcher(RedisConnection);
            watcher.SetUpdateCallback(() => callback.TrySetResult(1));

            watcher.Update();

            Assert.IsFalse(callback.Task.Wait(500), "The watcher shouldn't receive its self messages");
        }
    }
}
