using Domain.Entities;

namespace Application.Interfaces
{
    public interface ISendMessageCommand
    {
        Task<ChatMessage> ExecuteAsync(int chatRoomId, int senderId, string message);
    }
}

