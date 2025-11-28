using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Queries
{
    public class GetChatMessagesQuery : IGetChatMessagesQuery
    {
        private readonly AppDbContext _context;

        public GetChatMessagesQuery(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChatMessageRequest>> ExecuteAsync(int chatRoomId, int userId, int skip, int take)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.ChatRoomId == chatRoomId)
                .OrderByDescending(m => m.SendAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            // Verificar que el usuario tenga acceso a esta sala
            var room = await _context.ChatRooms
                .FirstOrDefaultAsync(r => r.Id == chatRoomId && 
                                         (r.DoctorID == userId || r.PatientId == userId));

            if (room == null)
            {
                return new List<ChatMessageRequest>();
            }

            return messages.Select(m =>
            {
                var sendAtUtc = m.SendAt.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(m.SendAt, DateTimeKind.Utc)
                    : m.SendAt.ToUniversalTime();

                return new ChatMessageRequest(
                    m.id,
                    m.ChatRoomId,
                    m.SenderId,
                    "Usuario", // TODO: Obtener nombre del sender
                    m.Message ?? "",
                    sendAtUtc,
                    m.IsRead
                );
            }).OrderBy(m => m.SendAt).ToList();
        }
    }
}

