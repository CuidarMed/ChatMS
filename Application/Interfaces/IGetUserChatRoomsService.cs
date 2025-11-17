using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IGetUserChatRoomsService
    {
        Task<List<ChatRoomRequest>> GetUserChatRoomsAsync(int userId);
    }
}
