using Application.DTOs;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class GetUserChatRoomsService : IGetUserChatRoomsService
    {
        private readonly IGetUserChatRoomsQuery getUserChatRoomsQuery;

        public GetUserChatRoomsService(IGetUserChatRoomsQuery getUserChatRoomsQuery)
        {
            this.getUserChatRoomsQuery = getUserChatRoomsQuery;
        }
        public async Task<List<ChatRoomRequest>> GetUserChatRoomsAsync(int userId)
        {
            return await getUserChatRoomsQuery.ExecuteAsync(userId);
        }
    }
}
