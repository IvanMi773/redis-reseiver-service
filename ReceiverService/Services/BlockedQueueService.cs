using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReceiverService.Entities;

namespace ReceiverService.Services
{
    public class BlockedQueueService
    {
        private readonly ConcurrentQueue<ExtendedRoot> _collection;
        private readonly ILogger<BlockedQueueService> _logger;

        public int CountOfElements { get; set; }

        public BlockedQueueService(ILogger<BlockedQueueService> logger)
        {
            _logger = logger;
            _collection = new ConcurrentQueue<ExtendedRoot>();
        }

        public async Task Add(ExtendedRoot root)
        {
            Task t1 = Task.Run(() =>
            {
                _collection.Enqueue(root);
                CountOfElements = _collection.Count;
                
                _logger.LogInformation("complete adding");
            });

            await Task.WhenAll(t1);
        }

        public ExtendedRoot Take()
        {
            _collection.TryPeek(out var root);
            CountOfElements = _collection.Count;

            return root;
        }

        public void Clear()
        {
            _collection.Clear();
        }
    }
}