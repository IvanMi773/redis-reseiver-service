using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ReceiverService.Services
{
    public class ServiceBusReceiverService : IHostedService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ServiceBusReceiverService> _logger;

        public ServiceBusReceiverService(HttpClient httpClient, ILogger<ServiceBusReceiverService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        using var requestMessage = new HttpRequestMessage(HttpMethod.Delete,
                            "https://test-bus-32.servicebus.windows.net/test-topic/subscriptions/subscr/messages/head");
                        requestMessage.Headers.Add("Authorization", "SharedAccessSignature sr=https%3a%2f%2ftest-bus-32.servicebus.windows.net%2ftest-topic%2f&sig=2T6rP3o6VUlEsBBsMQQ8D1SUH0UUmpVgyXbVJUzJNg4%3d&se=1632838452&skn=RootManageSharedAccessKey");
                    
                        var httpResponseMessage = await _httpClient.SendAsync(requestMessage);
                        string responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(responseBody))
                        {
                            _logger.LogInformation(responseBody);
                        }
                    }
                    catch (HttpRequestException)
                    {
                        _logger.LogError("http error");
                    }
                }
            });
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}