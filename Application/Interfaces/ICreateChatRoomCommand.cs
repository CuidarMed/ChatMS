using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ICreateChatRoomCommand
    {
        Task<ChatRoom> ExecuteAsync(CreateChatRoomRequest request);
    }
}

