using System;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ReceiverService.Providers
{
    public class RedisProvider : IRedisProvider
    {
        private long _lastReconnectTicks = DateTimeOffset.MinValue.UtcTicks;
        private int RetryMaxAttempts => 5;
        private DateTimeOffset _firstErrorTime = DateTimeOffset.MinValue;
        private DateTimeOffset _previousErrorTime = DateTimeOffset.MinValue;
        private TimeSpan ReconnectMinFrequency => TimeSpan.FromSeconds(60);
        private TimeSpan ReconnectErrorThreshold => TimeSpan.FromSeconds(30);
        private Lazy<ConnectionMultiplexer> _lazyConnection;
        private ConnectionMultiplexer Connection => _lazyConnection.Value;
        private readonly object _reconnectLock = new object();
        private readonly IConfiguration _configuration;
        private readonly ILogger<RedisProvider> _logger;

        public RedisProvider(IConfiguration configuration, ILogger<RedisProvider> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _lazyConnection = CreateConnection();
        }

        private Lazy<ConnectionMultiplexer> CreateConnection()
        {
            return new Lazy<ConnectionMultiplexer>(() =>
            {
                var cacheConnection = _configuration["CacheConnection"];
                return ConnectionMultiplexer.Connect(cacheConnection);
            });
        }

        private void CloseConnection(Lazy<ConnectionMultiplexer> oldConnection)
        {
            if (oldConnection == null)
                return;

            try
            {
                oldConnection.Value.Close();
            }
            catch (Exception)
            {
                _logger.LogError("Error while closing connection");
            }
        }

        private void ForceReconnect()
        {
            var utcNow = DateTimeOffset.UtcNow;
            var previousTicks = Interlocked.Read(ref _lastReconnectTicks);
            var previousReconnectTime = new DateTimeOffset(previousTicks, TimeSpan.Zero);
            var elapsedSinceLastReconnect = utcNow - previousReconnectTime;

            if (elapsedSinceLastReconnect < ReconnectMinFrequency)
                return;

            lock (_reconnectLock)
            {
                utcNow = DateTimeOffset.UtcNow;
                elapsedSinceLastReconnect = utcNow - previousReconnectTime;

                if (_firstErrorTime == DateTimeOffset.MinValue)
                {
                    _firstErrorTime = utcNow;
                    _previousErrorTime = utcNow;
                    return;
                }

                if (elapsedSinceLastReconnect < ReconnectMinFrequency)
                    return; 

                var elapsedSinceFirstError = utcNow - _firstErrorTime;
                var elapsedSinceMostRecentError = utcNow - _previousErrorTime;

                var shouldReconnect =
                    elapsedSinceFirstError >=
                    ReconnectErrorThreshold 
                    && elapsedSinceMostRecentError <=
                    ReconnectErrorThreshold;

                _previousErrorTime = utcNow;

                if (!shouldReconnect)
                    return;

                _firstErrorTime = DateTimeOffset.MinValue;
                _previousErrorTime = DateTimeOffset.MinValue;

                var oldConnection = _lazyConnection;
                CloseConnection(oldConnection);
                _lazyConnection = CreateConnection();
                Interlocked.Exchange(ref _lastReconnectTicks, utcNow.UtcTicks);
            }
        }

        private T BasicRetry<T>(Func<T> func)
        {
            var reconnectRetry = 0;
            var disposedRetry = 0;

            while (true)
            {
                try
                {
                    return func();
                }
                catch (Exception ex) when (ex is RedisConnectionException || ex is SocketException)
                {
                    reconnectRetry++;
                    if (reconnectRetry > RetryMaxAttempts)
                        _logger.LogError("Error while retrying to open connection");
                    ForceReconnect();
                }
                catch (ObjectDisposedException)
                {
                    disposedRetry++;
                    if (disposedRetry > RetryMaxAttempts)
                        _logger.LogError("Error while retrying to open connection");
                }
            }
        }

        public IDatabase GetDatabase()
        {
            return BasicRetry(() => Connection.GetDatabase());
        }
    }
}