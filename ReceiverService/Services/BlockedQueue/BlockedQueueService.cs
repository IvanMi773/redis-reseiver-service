using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ReceiverService.Entities;

namespace ReceiverService.Services.BlockedQueue
{
    public class BlockedQueueService : IBlockedQueueService
    {
        private readonly BlockingCollection<Root> _collection;
        private readonly ILogger<BlockedQueueService> _logger;

        public BlockedQueueService(ILogger<BlockedQueueService> logger)
        {
            _logger = logger;
            _collection = new BlockingCollection<Root>();
        }
        
        public void Add(Root root)
        {
            _collection.TryAdd(root);
            
            _logger.LogInformation("complete adding");
        }

        public Root Take(int milliseconds)
        {
            _collection.TryTake(out var root, milliseconds);
            return root;
        }
    }
}