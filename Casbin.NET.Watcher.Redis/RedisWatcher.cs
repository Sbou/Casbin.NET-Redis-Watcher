using System;
using NetCasbin.Persist;
using StackExchange.Redis;

namespace Redis.Casbin.NET
{
    /// <summary>
    /// Casbin watcher which uses a Redis cache with the pub/sub pattern
    /// </summary>
    public class RedisWatcher : IWatcher
    {
        private readonly IConnectionMultiplexer connection;
        private ISubscriber publisher;
        private Action callback;

        private const string channelName = "CasbinUpdate";

        private readonly string localID = Guid.NewGuid().ToString();

        /// <summary>
        /// Instanciate a new RedisWatcher
        /// <para>See. <see href="https://stackexchange.github.io/StackExchange.Redis/Configuration"/> for the connection string parameters</para>
        /// </summary>
        /// <param name="connectionString">a comma-delimited configuration string</param>
        /// <exception cref="StackExchange.Redis.RedisConnectionException"/>
        public RedisWatcher(string connectionString)
        {
            var configuration = ConfigurationOptions.Parse(connectionString);

            // initial a connection
            connection = ConnectionMultiplexer.Connect(configuration);

            Subscribe();
        }

        /// <summary>
        /// Instanciate a new RedisWatcher
        /// </summary>
        /// <param name="connection">An existing redis connection</param>
        public RedisWatcher(IConnectionMultiplexer connection)
        {
            this.connection = connection;

            Subscribe();
        }

        private void Subscribe()
        {
            publisher = connection.GetSubscriber();

            publisher.Subscribe(channelName, (channel, value) =>
            {
                if (value != localID)
                {
                    callback?.Invoke();
                }
            });
        }

        /// <summary>
        /// Set the callback to trigger when a message is received
        /// </summary>
        /// <param name="callback"></param>
        public void SetUpdateCallback(Action callback)
        {
            this.callback = callback;
        }

        /// <summary>
        /// Publish a message to prevent other instances
        /// </summary>
        public void Update()
        {
            publisher.PublishAsync(channelName, localID);
        }

        /// <summary>
        /// Close all the subscription and the connection
        /// </summary>
        public void Close()
        {
            publisher?.UnsubscribeAll();
            connection?.Close();
        }
    }
}
