using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HandleMyQueue.Models;
using HandleMyQueue.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace HandleMyQueue.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QueuesController : ControllerBase
    {
        private readonly QueuesService _queuesService;

        public QueuesController(QueuesService queuesService)
        {
            _queuesService = queuesService;
        }

        [HttpGet("getAllQueues")]
        public async Task<List<QueueDto>> GetQueues()
        {
            var username = User.Claims.First(c => c.Type.Equals("preferred_username")).Value;
            return await _queuesService.GetAllQueuesForUser(username);
        }

        [HttpGet("countAllQueues")]
        public async Task<long> CountQueues()
        {
            var username = User.Claims.First(c => c.Type.Equals("preferred_username")).Value;
            return await _queuesService.CountAllQueuesForUser(username);
        }

        [HttpPost]
        public void Create(QueueDto queueDto)
        {
            var username = User.Claims.First(c => c.Type.Equals("preferred_username")).Value;
            _queuesService.Create(queueDto, username);
        }
    }
}