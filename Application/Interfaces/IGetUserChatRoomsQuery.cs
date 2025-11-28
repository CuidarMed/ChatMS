using Application.DTOs;

namespace Application.Interfaces
{
    public interface IGetUserChatRoomsQuery
    {
        Task<List<ChatRoomRequest>> ExecuteAsync(int userId);
    }
}

