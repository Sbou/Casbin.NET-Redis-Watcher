using System;
using System.Threading.Tasks;
using Casbin.Persist;
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

            // initiate a connection
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

            publisher.Subscribe(RedisChannel.Literal(channelName), (channel, value) =>
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
        /// Set the callback to trigger when a message is received
        /// </summary>
        /// <param name="callback"></param>
        public void SetUpdateCallback(Func<Task> callback)
        {
            this.callback = () => { Task.Run(callback); };
        }

        /// <summary>
        /// Publish a message to prevent other instances
        /// </summary>
        public void Update()
        {
            publisher.PublishAsync(RedisChannel.Literal(channelName), localID);
        }

        /// <summary>
        /// Publish a message to prevent other instances
        /// </summary>
        public Task UpdateAsync()
        {
            return publisher.PublishAsync(RedisChannel.Literal(channelName), localID);
        }

        /// <summary>
        /// Close all the subscription and the connection
        /// </summary>
        public void Close()
        {
            publisher?.UnsubscribeAll();
            connection?.Close();
        }

        /// <summary>
        /// Close all the subscription and the connection
        /// </summary>
        public async Task CloseAsync()
        {
            await publisher?.UnsubscribeAllAsync();
            await connection?.CloseAsync();
        }

        /// <summary>
        /// This callback is not implemented because we only use the Redis to notify the other instances and not to store the modifications
        /// </summary>
        /// <param name="callback"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void SetUpdateCallback(Action<IPolicyChangeMessage> callback) => throw new NotImplementedException();

        /// <summary>
        /// This callback is not implemented because we only use the Redis to notify the other instances and not to store the modifications
        /// </summary>
        /// <param name="callback"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void SetUpdateCallback(Func<IPolicyChangeMessage, Task> callback) => throw new NotImplementedException();

        /// <summary>
        /// Publish a message to prevent other instances
        /// </summary>
        /// <param name="message"></param>
        /// <remarks>The given message is not send in the notification</remarks>
        public void Update(IPolicyChangeMessage message) => Update();

        /// <summary>
        /// Publish a message to prevent other instances
        /// </summary>
        /// <param name="message"></param>
        /// <remarks>The given message is not send in the notification</remarks>
        public Task UpdateAsync(IPolicyChangeMessage message) => UpdateAsync();
    }
}
