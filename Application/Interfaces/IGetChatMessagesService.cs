using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IGetChatMessagesService
    {
        Task<List<ChatMessageRequest>> GetChatMessagesAsync(int chatRoomId, int userId, int skip = 0, int take = 50);
    }
}
