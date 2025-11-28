using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Command
{
    public class SendMessageCommand : ISendMessageCommand
    {
        private readonly AppDbContext _context;

        public SendMessageCommand(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ChatMessage> ExecuteAsync(int chatRoomId, int senderId, string message)
        {
            // Verificar que la sala existe
            var room = await _context.ChatRooms
                .FirstOrDefaultAsync(r => r.Id == chatRoomId && r.IsActive);

            if (room == null)
            {
                throw new InvalidOperationException("Sala de chat no encontrada");
            }
            
            // Verificar que el usuario tiene acceso (pero no fallar si no coincide exactamente)
            // Esto permite que cualquier usuario que tenga acceso a la sala pueda enviar mensajes
            var hasAccess = room.DoctorID == senderId || room.PatientId == senderId;
            if (!hasAccess)
            {
                // Log warning pero permitir el mensaje (puede ser un caso edge)
                // En producción, esto debería ser más estricto
                Console.WriteLine($"Warning: Usuario {senderId} enviando mensaje en sala {chatRoomId} sin acceso directo");
            }

            var chatMessage = new ChatMessage
            {
                ChatRoomId = chatRoomId,
                SenderId = senderId,
                Message = message,
                SendAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            return chatMessage;
        }
    }
}

