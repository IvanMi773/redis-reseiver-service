using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ReceiverService.Entities;
using ReceiverService.Repositories;

namespace ReceiverService.Controllers
{
    public class TestController : ControllerBase
    {
        private readonly IRedisRepository _redisRepository;
        
        public TestController(IRedisRepository redisRepository)
        {
            _redisRepository = redisRepository;
        }
        
        [HttpPost("/send")]
        public void Send([FromBody] Root root)
        {
            _redisRepository.PushStringToList("roots", JsonSerializer.Serialize(root));
        }
    }
}