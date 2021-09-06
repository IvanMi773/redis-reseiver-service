using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using ReceiverService.Entities;

namespace ReceiverService.Services
{
    public class BlockedQueueService
    {
        public BlockingCollection<ExtendedRoot> bc = new BlockingCollection<ExtendedRoot>();

        public async Task Add(ExtendedRoot root)
        {
            Task t1 = Task.Run(() =>
            {
                bc.Add(root);
                bc.CompleteAdding();
            });

            Task t2 = Task.Run(() =>
            {
                try
                {
                    while (true) Console.WriteLine(bc.Take());
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("That's All!");
                }
            });

            await Task.WhenAll(t1, t2);
        }
    }
}