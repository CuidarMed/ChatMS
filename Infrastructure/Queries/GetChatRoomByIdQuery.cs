using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Queries
{
    public class GetChatRoomByIdQuery : IGetChatRoomByIdQuery
    {
        private readonly AppDbContext _context;
        private readonly IUserCacheService _userCache;

        public GetChatRoomByIdQuery(AppDbContext context, IUserCacheService userCache)
        {
            _context = context;
            _userCache = userCache;
        }

        public async Task<ChatRoomRequest> ExecuteAsync(int chatRoomId, int userId)
        {
            var chatRoom = await _context.ChatRooms
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == chatRoomId &&
                    (c.DoctorId == userId || c.PatientId == userId));

            if (chatRoom == null)
                return null!; // null-forgiving para mantener la firma actual. Considera cambiar la interfaz a ChatRoomRequest?.

            // Obtener info de usuarios desde caché
            var doctor = await _userCache.GetUserAsync(chatRoom.DoctorId);
            var patient = await _userCache.GetUserAsync(chatRoom.PatientId);

            var lastMessage = chatRoom.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
            var unreadCount = chatRoom.Messages.Count(m => m.SenderId != userId && !m.IsRead);

            // Crear la instancia usando el constructor con parámetros (evita el inicializador que falla)
            // Orden esperado por el compilador: (int, int, string, int, string, DateTime, string, DateTime?, int)
            return new ChatRoomRequest(
                chatRoom.Id,
                chatRoom.DoctorId,
                doctor?.Name ?? "Doctor",
                chatRoom.PatientId,
                patient?.Name ?? "Paciente",
                chatRoom.CreatedAt,
                lastMessage?.Message ?? string.Empty,
                lastMessage?.SentAt,
                unreadCount
            );
        }
    }
}
