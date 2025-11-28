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

        public async Task ExecuteAsync(int chatRoomId, int userId, string userRole)
        {
            // Marcar como leídos los mensajes del OTRO rol
            var messages = await _context.ChatMessages
                .Where(m => m.ChatRoomId == chatRoomId &&
                           m.SenderRole != userRole &&  // ← CLAVE: mensajes del OTRO participante
                           !m.IsRead)
                .ToListAsync();

            Console.WriteLine($"📝 Mensajes a marcar como leídos: {messages.Count}");
            Console.WriteLine($"📝 userRole recibido: {userRole}");

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}