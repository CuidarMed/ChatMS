namespace Application.Interfaces
{
    public interface IMarkMessagesAsReadCommand
    {
        Task ExecuteAsync(int chatRoomId, int userId);
    }
}

