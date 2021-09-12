namespace ReceiverService.Services.Events
{
    public interface IEventConsumerService
    {
        public void ConsumeMessagesFromQueue();
    }
}