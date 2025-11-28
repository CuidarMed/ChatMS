using Application.DTOs;

namespace Application.Interfaces
{
    public interface IGetChatRoomByIdQuery
    {
        Task<ChatRoomRequest> ExecuteAsync(int chatRoomId, int userId);
    }
}

