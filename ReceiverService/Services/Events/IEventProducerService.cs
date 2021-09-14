namespace ReceiverService.Services.Events
{
    public interface IEventProducerService
    {
        public void ProduceMessageToQueue();
    }
}