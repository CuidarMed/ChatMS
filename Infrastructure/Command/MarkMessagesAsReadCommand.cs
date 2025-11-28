using Application.Interfaces;
using Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Command
{
    public class MarkMessagesAsReadCommand : IMarkMessagesAsReadCommand
    {
        private readonly AppDbContext _context;

        public MarkMessagesAsReadCommand(AppDbContext context)
        {
            _context = context;
        }

        public async Task ExecuteAsync(int chatRoomId, int userId)
        {
            // Verificar que la sala existe
            var room = await _context.ChatRooms
                .FirstOrDefaultAsync(r => r.Id == chatRoomId && r.IsActive);

            if (room == null)
            {
                // Si la sala no existe, simplemente retornar sin error (puede ser que se eliminó)
                return;
            }

            // Verificar que el usuario tiene acceso (pero no fallar si no hay mensajes)
            var hasAccess = room.DoctorID == userId || room.PatientId == userId;
            if (!hasAccess)
            {
                // Si no tiene acceso, retornar sin error (evitar exponer información)
                return;
            }

            // Marcar todos los mensajes no leídos que no fueron enviados por el usuario
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.ChatRoomId == chatRoomId && 
                           m.SenderId != userId && 
                           !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}

