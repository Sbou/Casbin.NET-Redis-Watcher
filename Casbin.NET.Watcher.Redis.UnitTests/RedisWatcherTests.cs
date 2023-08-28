using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Redis.Casbin.NET;
using StackExchange.Redis;

namespace Casbin.NET.Watcher.Redis.UnitTests
{
    [TestClass]
    public class RedisWatcherTests : RedisTestBase
    {
        public IConnectionMultiplexer GetConnection(SubscriptionType subscriptionType)
        {
#if TRUEREDIS
            return ConnectionMultiplexer.Connect("localhost:6379");
#else
            return GetMockedConnection(subscriptionType);
#endif
        }

        [TestMethod]
        public void BadConnectionStringTest()
        {
            var connectionString = "unkonwnhost:6379";

            Assert.ThrowsException<StackExchange.Redis.RedisConnectionException>(() => new RedisWatcher(connectionString));
        }

#if !TRUEREDIS
        [TestMethod]
        public void CloseConnectionTest()
        {
            var connection = GetConnection(SubscriptionType.Publisher);

            var watcher = new RedisWatcher(connection);

            watcher.Close();

            Mock.Get(connection).Verify(con => con.Close(It.IsAny<bool>()), Times.Once);
        }

        [TestMethod]
        public async Task CloseConnectionTestAsync()
        {
            var connection = GetConnection(SubscriptionType.Publisher);

            var watcher = new RedisWatcher(connection);

            await watcher.CloseAsync();

            Mock.Get(connection).Verify(con => con.CloseAsync(It.IsAny<bool>()), Times.Once);
        }
#endif

        [TestMethod]
        public void NominalTest()
        {
            var callback = new TaskCompletionSource<int>();

            var watcher = new RedisWatcher(GetConnection(SubscriptionType.Subscriber));
            watcher.SetUpdateCallback(() => callback.TrySetResult(1));

            var watcher2 = new RedisWatcher(GetConnection(SubscriptionType.Publisher));
            watcher2.Update();

            Assert.IsTrue(callback.Task.Wait(300), "The first watcher didn't receive the notification");
        }

        [TestMethod]
        public void IgnoreSelfMessageTest()
        {
            var callback = new TaskCompletionSource<int>();

            var watcher = new RedisWatcher(GetConnection(SubscriptionType.Both));
            watcher.SetUpdateCallback(() => callback.TrySetResult(1));

            watcher.Update();

            Assert.IsFalse(callback.Task.Wait(500), "The watcher shouldn't receive its self messages");
        }

        [TestMethod]
        public void CallbackNull()
        {
            var watcher = new RedisWatcher(GetConnection(SubscriptionType.Subscriber));

            var watcher2 = new RedisWatcher(GetConnection(SubscriptionType.Publisher));
            watcher2.Update();
        }

        [TestMethod]
        public async Task NominalTestAsync()
        {
            var validation = new TaskCompletionSource<int>();

            Task callback()
            {
                validation.SetResult(1);
                return Task.CompletedTask;
            }

            var watcher = new RedisWatcher(GetConnection(SubscriptionType.Subscriber));
            watcher.SetUpdateCallback(callback);

            var watcher2 = new RedisWatcher(GetConnection(SubscriptionType.Publisher));
            await watcher2.UpdateAsync();

            Assert.IsTrue(validation.Task.Wait(300), "The first watcher didn't receive the notification");
        }
    }
}
