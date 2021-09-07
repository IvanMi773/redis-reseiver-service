using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReceiverService.Entities;

namespace ReceiverService.Services
{
    public class BlockedQueueService
    {
        private ConcurrentQueue<ExtendedRoot> collection;
        
        public int CountOfElements { get; set; }

        public BlockedQueueService()
        {
            collection = new ConcurrentQueue<ExtendedRoot>();
        }

        public async Task Add(ExtendedRoot root)
        {
            Task t1 = Task.Run(() =>
            {
                collection.Enqueue(root);

                CountOfElements = collection.Count;
                
                Console.WriteLine("complete adding");
            });

            await Task.WhenAll(t1);
        }

        public ExtendedRoot Take()
        {
            collection.TryDequeue(out var root);

            return root;
        }
    }
}