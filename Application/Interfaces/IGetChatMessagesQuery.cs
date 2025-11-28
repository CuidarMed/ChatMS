using Application.DTOs;

namespace Application.Interfaces
{
    public interface IGetChatMessagesQuery
    {
        Task<List<ChatMessageRequest>> ExecuteAsync(int chatRoomId, int userId, int skip, int take);
    }
}

