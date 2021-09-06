using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ReceiverService.Entities;
using StackExchange.Redis;

namespace ReceiverService.Controllers
{
    public class TestController : ControllerBase
    {
        private static IConfiguration _configuration;
        
        public TestController(IConfiguration configuration)
        {
            if (_configuration == null)
            {
                _configuration = configuration;
            }
        }
        
        [HttpGet("/send")]
        public void Send()
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_configuration["CacheConnection"]);
            ISubscriber sub = redis.GetSubscriber();
            
            var root = new Root("dsa", "fdsafd", "fdas", "fdsa", "fdsa");
            
            sub.Publish("roots", JsonSerializer.Serialize(root));
        }
    }
}