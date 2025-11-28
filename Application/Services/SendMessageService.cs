using Application.DTOs;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class SendMessageService : ISendMessageService
    {
        private readonly ISendMessageCommand sendMessageCommand;

        public SendMessageService(ISendMessageCommand sendMessageCommand)
        {
            this.sendMessageCommand = sendMessageCommand;
        }
        public async Task<ChatMessageRequest> SendMessageAsync(int chatRoomId, int senderId, string message)
        {
            var chatMessage = await sendMessageCommand.ExecuteAsync(chatRoomId, senderId, message);

            var sendAtUtc = chatMessage.SendAt.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(chatMessage.SendAt, DateTimeKind.Utc)
                : chatMessage.SendAt.ToUniversalTime();

            return new ChatMessageRequest(
                chatMessage.id,
                chatMessage.ChatRoomId,
                chatMessage.SenderId,
                "Usuario", // TODO: Obtener nombre del sender
                chatMessage.Message ?? "",
                sendAtUtc,
                chatMessage.IsRead
            );
        }
    }
}
