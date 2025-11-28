using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Queries
{
    public class GetChatRoomByIdQuery : IGetChatRoomByIdQuery
    {
        private readonly AppDbContext _context;

        public GetChatRoomByIdQuery(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ChatRoomRequest> ExecuteAsync(int chatRoomId, int userId)
        {
            var room = await _context.ChatRooms
                .Include(r => r.Messages)
                .FirstOrDefaultAsync(r => r.Id == chatRoomId && 
                                         (r.DoctorID == userId || r.PatientId == userId) &&
                                         r.IsActive);

            if (room == null)
            {
                return null;
            }

            // Obtener nombres (simplificado - en producción deberías obtenerlos de DirectoryMS)
            var lastMessage = room.Messages.OrderByDescending(m => m.SendAt).FirstOrDefault();

            return new ChatRoomRequest(
                room.Id,
                room.PatientId,
                "Paciente", // TODO: Obtener de DirectoryMS
                room.DoctorID,
                "Doctor", // TODO: Obtener de DirectoryMS
                room.AppointmentId, // Incluir AppointmentId
                room.CreatedAt,
                lastMessage?.Message ?? "",
                lastMessage?.SendAt,
                room.Messages.Count(m => !m.IsRead && m.SenderId != userId),
                lastMessage?.SenderId // LastSenderId: ID del usuario que envió el último mensaje
            );
        }
    }
}

