using Application.DTOs;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CreateRoomService : ICreateRoomService
    {
        private readonly ICreateChatRoomCommand createChatRoomCommand;
        private readonly IGetChatRoomByIdQuery getChatRoomByIdQuery;

        public CreateRoomService(ICreateChatRoomCommand createChatRoomCommand, IGetChatRoomByIdQuery getChatRoomByIdQuery,)
        {
            this.createChatRoomCommand = createChatRoomCommand;
            this.getChatRoomByIdQuery = getChatRoomByIdQuery;
        }
        public async Task<ChatRoomRequest> CreateChatRoomAsync(CreateChatRoomRequest request)
        {
            var chatRoom = await createChatRoomCommand.ExecuteAsync(request);
            return await getChatRoomByIdQuery.ExecuteAsync(chatRoom.Id, request.DoctorId);
        }
    }
}
