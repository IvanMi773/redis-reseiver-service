using System.Text.Json;
using System.Threading.Tasks;
using ReceiverService.Entities;
using ReceiverService.Mappers;

namespace ReceiverService.Services
{
    public class ProcessMessagesService : IProcessMessagesService
    {
        private readonly IBlockedQueueService _blockedQueueService;
        private readonly IServiceBusSenderService _serviceBusSenderService;

        public ProcessMessagesService(IBlockedQueueService blockedQueueService, IServiceBusSenderService serviceBusSenderService)
        {
            _blockedQueueService = blockedQueueService;
            _serviceBusSenderService = serviceBusSenderService;
        }

        public async Task GetMessagesFromQueueAndSendToServiceBus()
        {
            var messages = new string[_blockedQueueService.CountOfElements()];
            for (var i = 0; i < _blockedQueueService.CountOfElements(); i++)
            {
                var extendedRoot = RootToExtendedRootMapper.Map(_blockedQueueService.Take(), 43);
                var msg = JsonSerializer.Serialize(extendedRoot);
                messages[i] = msg;
            }

            _blockedQueueService.Clear();
            await _serviceBusSenderService.SendMessage(messages);
        }
    }
}