using Application.DTOs;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class GetChatMessagesService : IGetChatMessagesService
    {
        private readonly IGetChatMessagesQuery _getChatMessagesQuery;

        public GetChatMessagesService(IGetChatMessagesQuery getChatMessagesQuery)
        {
            _getChatMessagesQuery = getChatMessagesQuery;
        }
        public async Task<List<ChatMessageRequest>> GetChatMessagesAsync(GetMessagesRequestDto request)
        {
            return await _getChatMessagesQuery.ExecuteAsync(request);
        }
    }
}
