using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ReceiverService.Entities;
using ReceiverService.Repositories;
using StackExchange.Redis;

namespace ReceiverService.Controllers
{
    public class TestController : ControllerBase
    {
        private IConfiguration _configuration;
        private readonly IRedisRepository _redisRepository;
        
        public TestController(IConfiguration configuration, IRedisRepository redisRepository)
        {
            _configuration = configuration;
            _redisRepository = redisRepository;
        }
        
        [HttpPost("/send")]
        public void Send([FromBody] Root root)
        {
            _redisRepository.PushStringToList("roots", JsonSerializer.Serialize(root));
        }
    }
}