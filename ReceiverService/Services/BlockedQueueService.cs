using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ReceiverService.Entities;

namespace ReceiverService.Services
{
    public class BlockedQueueService : IBlockedQueueService
    {
        private readonly ConcurrentQueue<Root> _collection;
        private readonly ILogger<BlockedQueueService> _logger;

        public BlockedQueueService(ILogger<BlockedQueueService> logger)
        {
            _logger = logger;
            _collection = new ConcurrentQueue<Root>();
        }

        public void Add(Root t)
        {
            _collection.Enqueue(t);
            
            _logger.LogInformation("complete adding");
        }

        public Root Take()
        {
            _collection.TryDequeue(out var root);

            return root;
        }

        public int CountOfElements()
        {
            return _collection.Count;
        }
    }
}