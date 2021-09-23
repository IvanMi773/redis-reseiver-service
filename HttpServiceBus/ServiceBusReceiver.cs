using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HttpServiceBus
{
    public class ServiceBusReceiver : IHostedService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ServiceBusReceiver> _logger;

        public ServiceBusReceiver(HttpClient httpClient, ILogger<ServiceBusReceiver> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                List<string> events = new List<string>();
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
                            events.Add(responseBody);
                            if (events.Count == 20)
                            {
                                Console.WriteLine("_______________________________________________");
                                foreach (var ev in events)
                                {
                                    Console.WriteLine(ev);
                                }
                                Console.WriteLine("_______________________________________________");
                                events.Clear();
                            }
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
            return Task.CompletedTask;
        }
    }
}