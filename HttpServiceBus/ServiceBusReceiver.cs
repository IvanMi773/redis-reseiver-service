using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReceiverService.Entities;

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
                List<ExtendedRoot> events = new List<ExtendedRoot>();
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
                            Console.WriteLine("Events count: " + events.Count);
                            events.Add(JsonSerializer.Deserialize<ExtendedRoot>(responseBody));
                            if (events.Count == 500)
                            {
                                var eventsWithId1 = new List<ExtendedRoot>();
                                var eventsWithId2 = new List<ExtendedRoot>();
                                var eventsWithId3 = new List<ExtendedRoot>();
                                
                                foreach (var ev in events)
                                {
                                    if (ev.Id == "Id number 1 (selected)")
                                    {
                                        eventsWithId1.Add(ev);
                                    } else if (ev.Id == "Id number 2")
                                    {
                                        eventsWithId2.Add(ev);
                                    } else if (ev.Id == "Id number 3")
                                    {
                                        eventsWithId3.Add(ev);
                                    }
                                }

                                var countOfIncorrectFor1 = 0;
                                for (int i = 0; i < eventsWithId1.Count - 1; i++)
                                {
                                    DateTime.TryParse(eventsWithId1[i].Timestamp, out var time1);
                                    DateTime.TryParse(eventsWithId1[i + 1].Timestamp, out var time2);
                                    if (DateTime.Compare(time1, time2) > 0)
                                        countOfIncorrectFor1++;
                                }
                                
                                var countOfIncorrectFor2 = 0;
                                for (int i = 0; i < eventsWithId2.Count - 1; i++)
                                {
                                    DateTime.TryParse(eventsWithId2[i].Timestamp, out var time1);
                                    DateTime.TryParse(eventsWithId2[i + 1].Timestamp, out var time2);
                                    if (DateTime.Compare(time1, time2) > 0)
                                        countOfIncorrectFor2++;
                                }
                                
                                var countOfIncorrectFor3 = 0;
                                for (int i = 0; i < eventsWithId3.Count - 1; i++)
                                {
                                    DateTime.TryParse(eventsWithId3[i].Timestamp, out var time1);
                                    DateTime.TryParse(eventsWithId3[i + 1].Timestamp, out var time2);
                                    if (DateTime.Compare(time1, time2) > 0)
                                        countOfIncorrectFor3++;
                                }
                                Console.WriteLine("_______________________________________________");
                                Console.WriteLine("Count of incorrect for id 1: " + countOfIncorrectFor1);
                                Console.WriteLine("Count of incorrect for id 2: " + countOfIncorrectFor2);
                                Console.WriteLine("Count of incorrect for id 3: " + countOfIncorrectFor3);
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