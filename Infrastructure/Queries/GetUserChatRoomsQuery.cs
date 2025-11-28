using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Queries
{
    public class GetUserChatRoomsQuery : IGetUserChatRoomsQuery
    {
        private readonly AppDbContext _context;

        public GetUserChatRoomsQuery(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChatRoomRequest>> ExecuteAsync(int userId)
        {
            // FIX DEFINITIVO: Soportar ambos casos (userId y IDs clínicos)
            // Las salas pueden estar guardadas con:
            // 1. DoctorID/PatientId = userId (FKs a Users.Id) - caso estándar
            // 2. DoctorID/PatientId = doctorId/patientId (IDs clínicos) - caso cuando se pasa participantId desde frontend
            // 3. Usuario participó enviando mensajes (SenderId = userId)
            
            // Buscar salas donde:
            // - El usuario es doctor o paciente directo (DoctorID/PatientId = userId)
            // - O el usuario envió mensajes en esa sala (participó activamente)
            var rooms = await _context.ChatRooms
                .AsNoTracking()
                .Include(r => r.Messages)
                .Where(r => r.IsActive && (
                    r.DoctorID == userId || 
                    r.PatientId == userId ||
                    r.Messages.Any(m => m.SenderId == userId)
                ))
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return rooms.Select(room =>
            {
                var lastMessage = room.Messages.OrderByDescending(m => m.SendAt).FirstOrDefault();
                return new ChatRoomRequest(
                    room.Id,
                    room.PatientId,
                    "Paciente", // TODO: Obtener de DirectoryMS
                    room.DoctorID,
                    "Doctor", // TODO: Obtener de DirectoryMS
                    room.AppointmentId,
                    room.CreatedAt,
                    lastMessage?.Message ?? "",
                    lastMessage?.SendAt,
                    // UnreadCount: contar mensajes no leídos donde el SenderId NO es el userId actual
                    // SenderId en ChatMessages es el userId (Auth), no el doctorId/patientId
                    room.Messages.Count(m => !m.IsRead && m.SenderId != userId),
                    // LastSenderId: ID del usuario que envió el último mensaje (para filtrar mensajes propios en frontend)
                    lastMessage?.SenderId
                );
            }).ToList();
        }
    }
}

