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
        private readonly IGetChatMessagesQuery getChatMessagesQuery;

        public GetChatMessagesService(IGetChatMessagesQuery getChatMessagesQuery)
        {
            this.getChatMessagesQuery = getChatMessagesQuery;
        }
        public async Task<List<ChatMessageRequest>> GetChatMessagesAsync(int chatRoomId, int userId, int skip = 0, int take = 50)
        {
            return await getChatMessagesQuery.ExecuteAsync(chatRoomId, userId, skip, take);
        }
    }
}
