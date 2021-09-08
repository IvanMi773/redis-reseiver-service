using System;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ReceiverService.Entities;
using ReceiverService.Repositories;

namespace ReceiverService.Controllers
{
    public class SenderController : ControllerBase
    {
        private readonly IRedisRepository _redisRepository;
        
        public SenderController(IRedisRepository redisRepository)
        {
            _redisRepository = redisRepository;
        }
        
        [HttpPost("/send")]
        public string Send([FromBody] Root root)
        {
            try
            {
                _redisRepository.PushStringToList("roots", JsonSerializer.Serialize(root));
                
                return "Ok";
            }
            catch (Exception)
            {
                return "Error while push message to redis";
            }
        }
    }
}