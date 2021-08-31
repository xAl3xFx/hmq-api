using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HandleMyQueue.Models;
using HandleMyQueue.Models.DTOs;
using HandleMyQueue.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace HandleMyQueue.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly MessagesService _messagesService;

        public MessagesController(MessagesService messagesService)
        {
            _messagesService = messagesService;
        }

        [HttpGet("getAllMessages")]
        public async Task<List<MessageDto>> GetQueues()
        {
            var username = User.Claims.First(c => c.Type.Equals("preferred_username")).Value;
            return await _messagesService.GetAllMessagesForUser(username);
        }

        [HttpGet("countAllMessages")]
        public async Task<MessageCountDto> CountQueues()
        {
            var username = User.Claims.First(c => c.Type.Equals("preferred_username")).Value;
            return await _messagesService.CountAllMessagesForUser(username);
        }

        [HttpPost]
        public void Create(MessageDto messageDto)
        {
            var username = User.Claims.First(c => c.Type.Equals("preferred_username")).Value;
            _messagesService.Create(messageDto, username);
        }
    }
}