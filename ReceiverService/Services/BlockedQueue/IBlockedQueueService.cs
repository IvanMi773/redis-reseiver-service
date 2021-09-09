using ReceiverService.Entities;

namespace ReceiverService.Services.BlockedQueue
{
    public interface IBlockedQueueService
    {
        public void Add(Root t);

        public Root Take();

        public int CountOfElements();
    }
}