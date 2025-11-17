using Application.DTOs;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class GetChatRoomService : IGetChatRoomService
    {
        private readonly IGetChatRoomByIdQuery getChatRoomByIdQuery;

        public GetChatRoomService(IGetChatRoomByIdQuery getChatRoomByIdQuery)
        {
            this.getChatRoomByIdQuery = getChatRoomByIdQuery;
        }
        public async Task<ChatRoomRequest> GetChatRoomAsync(int chatRoomId, int userId)
        {
            return await getChatRoomByIdQuery.ExecuteAsync(chatRoomId, userId);
        }
    }
}
