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
            // Primero determinar el rol del usuario que está leyendo
            var chatRoom = await _context.ChatRooms
                .FirstOrDefaultAsync(r => r.Id == chatRoomId);

            if (chatRoom == null) return;

            // Determinar qué rol tiene el usuario que lee
            string readerRole;
            if (chatRoom.DoctorId == userId)
            {
                readerRole = "Doctor";
            }
            else if (chatRoom.PatientId == userId)
            {
                readerRole = "Patient";
            }
            else
            {
                return; // Usuario no pertenece a esta sala
            }

            // Marcar como leídos los mensajes del OTRO rol (no los propios)
            var messages = await _context.ChatMessages
                .Where(m => m.ChatRoomId == chatRoomId &&
                           m.SenderRole != readerRole &&  // Mensajes del otro participante
                           !m.IsRead)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}