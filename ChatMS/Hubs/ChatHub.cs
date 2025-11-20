using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ChatMS.Hubs
{
    public class ChatHub : Hub
    {
        //private readonly IChatService _chatService;
        private readonly IGetChatRoomService _getChatRoomService;
        private readonly ISendMessageService _sendMessageService;
        private readonly IMarkMessagesAsReadService _markMessagesAsReadService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IGetChatRoomService getChatRoomService, ISendMessageService sendMessageService, IMarkMessagesAsReadService markMessagesAsReadService, ILogger<ChatHub> logger)
        {
            _getChatRoomService = getChatRoomService;
            _sendMessageService = sendMessageService;
            _markMessagesAsReadService = markMessagesAsReadService;
            _logger = logger;
        }

        public async Task JoinChatRoom(int chatRoomId, int userId)
        {
            try
            {
                // Verificar acceso
                var chatRoom = await _getChatRoomService.GetChatRoomAsync(chatRoomId, userId);
                if (chatRoom == null)
                {
                    await Clients.Caller.SendAsync("Error", "No tienes acceso a esta sala");
                    return;
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatRoomId}");
                await Clients.Caller.SendAsync("JoinedChatRoom", chatRoomId);

                _logger.LogInformation($"Usuario {userId} se unió a la sala {chatRoomId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al unirse a la sala de chat");
                await Clients.Caller.SendAsync("Error", "Error al unirse a la sala");
            }
        }

        public async Task LeaveChatRoom(int chatRoomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatRoomId}");
            await Clients.Caller.SendAsync("LeftChatRoom", chatRoomId);
        }

        public async Task SendMessage(SendMessageRequest dto)
        {
            _logger.LogInformation("HUB recibió mensaje: {msg}", dto.Message);
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Message))
                {
                    await Clients.Caller.SendAsync("Error", "El mensaje no puede estar vacío");
                    return;
                }

                // Enviar mensaje a través del servicio
                var messageDto = await _sendMessageService.SendMessageAsync(dto);

                // Notificar a todos en la sala
                await Clients.Group($"chat_{dto.ChatRoomId}").SendAsync("ReceiveMessage", messageDto);

                _logger.LogInformation($"Mensaje enviado en sala {dto.ChatRoomId} por usuario {dto.SenderId}");
            }
            catch (UnauthorizedAccessException ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar mensaje");
                await Clients.Caller.SendAsync("Error", "Error al enviar el mensaje");
            }
        }

        public async Task MarkAsRead(int chatRoomId, int userId)
        {
            try
            {
                await _markMessagesAsReadService.MarkMessagesAsReadAsync(chatRoomId, userId);
                await Clients.Group($"chat_{chatRoomId}").SendAsync("MessagesRead", chatRoomId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar mensajes como leídos");
            }
        }

        public async Task UserTyping(int chatRoomId, int userId, string userName)
        {
            await Clients.OthersInGroup($"chat_{chatRoomId}")
                .SendAsync("UserTyping", new { userId, userName });
        }

        public async Task UserStoppedTyping(int chatRoomId, int userId)
        {
            await Clients.OthersInGroup($"chat_{chatRoomId}")
                .SendAsync("UserStoppedTyping", userId);
        }

        public async Task UserOnline(int userId)
        {
            await Clients.All.SendAsync("UserOnline", userId);
        }

        public async Task UserOffline(int userId)
        {
            await Clients.All.SendAsync("UserOffline", userId);
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogWarning("SE CONECTÓ: " + Context.ConnectionId);
            return base.OnConnectedAsync();
        }
    }
}
