using Microsoft.AspNetCore.SignalR;
using Application.Interfaces;
using Application.DTOs;

namespace ChatMS.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ISendMessageService _sendMessageService;

        public ChatHub(ISendMessageService sendMessageService)
        {
            _sendMessageService = sendMessageService;
        }

        public async Task JoinChatRoom(int chatRoomId, int userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"room_{chatRoomId}");
            await Clients.Group($"room_{chatRoomId}").SendAsync("UserJoined", userId);
        }

        public async Task LeaveChatRoom(int chatRoomId, int userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room_{chatRoomId}");
            await Clients.Group($"room_{chatRoomId}").SendAsync("UserLeft", userId);
        }

        public async Task SendMessage(int chatRoomId, int senderId, string message)
        {
            try
            {
                // Guardar el mensaje en la base de datos
                var savedMessage = await _sendMessageService.SendMessageAsync(chatRoomId, senderId, message);
                
                if (savedMessage == null)
                {
                    throw new Exception("No se pudo guardar el mensaje en la base de datos");
                }
                
                // El mensaje se envía a todos en la sala
                var sendAtUtc = savedMessage.SendAt.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(savedMessage.SendAt, DateTimeKind.Utc)
                    : savedMessage.SendAt.ToUniversalTime();

                var messageData = new
                {
                    Id = savedMessage.Id,
                    ChatRoomId = chatRoomId,
                    SenderId = senderId,
                    SenderName = savedMessage.SenderName,
                    Message = message,
                    SendAt = sendAtUtc,
                    IsRead = savedMessage.IsRead
                };
                
                await Clients.Group($"room_{chatRoomId}").SendAsync("ReceiveMessage", messageData);
            }
            catch (Exception ex)
            {
                // Log del error en el servidor
                var logger = Context.GetHttpContext()?.RequestServices.GetService<ILogger<ChatHub>>();
                logger?.LogError(ex, "Error al enviar mensaje en sala {ChatRoomId}", chatRoomId);
                
                // Enviar error al cliente que intentó enviar
                await Clients.Caller.SendAsync("Error", "Error al enviar mensaje: " + ex.Message);
            }
        }

        public async Task UserTyping(int chatRoomId, int userId, string userName = null)
        {
            await Clients.GroupExcept($"room_{chatRoomId}", Context.ConnectionId)
                .SendAsync("UserTyping", new { userId = userId, userName = userName });
        }

        public async Task UserStoppedTyping(int chatRoomId, int userId)
        {
            await Clients.GroupExcept($"room_{chatRoomId}", Context.ConnectionId)
                .SendAsync("UserStoppedTyping", userId);
        }
    }
}
