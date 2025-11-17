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
        private readonly ISendMessageCommand _sendMessageCommand;
        private readonly IUserCacheService _userCache;

        public SendMessageService(ISendMessageCommand sendMessageCommand, IUserCacheService userCache)
        {
            _sendMessageCommand = sendMessageCommand;
            _userCache = userCache;
        }
        public async Task<ChatMessageRequest> SendMessageAsync(SendMessageRequest dto)
        {
            var chatMessage = await _sendMessageCommand.ExecuteAsync(dto);

            // Obtener info del sender desde caché
            var sender = await _userCache.GetUserAsync(chatMessage.SenderId);

            return new ChatMessageRequest(
                chatMessage.Id,
                chatMessage.ChatRoomId,
                chatMessage.SenderId,
                sender?.Name ?? "Usuario",
                chatMessage.Message,
                chatMessage.SentAt,
                chatMessage.IsRead
            );
        }
    }
}
