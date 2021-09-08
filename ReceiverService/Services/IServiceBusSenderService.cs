using System.Threading.Tasks;

namespace ReceiverService.Services
{
    public interface IServiceBusSenderService
    {
        public Task SendMessage(string[] messages);
    }
}