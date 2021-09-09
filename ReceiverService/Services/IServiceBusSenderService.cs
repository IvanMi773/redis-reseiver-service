using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReceiverService.Services
{
    public interface IServiceBusSenderService
    {
        public Task SendMessage(List<string> messages);
    }
}