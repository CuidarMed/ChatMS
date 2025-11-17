using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Queries
{
    public class GetChatMessagesQuery : IGetChatMessagesQuery
    {
        private readonly AppDbContext _context;
        private readonly IUserCacheService _userCache;

        public GetChatMessagesQuery(AppDbContext context, IUserCacheService userCache)
        {
            _context = context;
            _userCache = userCache;
        }

        public async Task<List<ChatMessageRequest>> ExecuteAsync(GetMessagesRequestDto request)
        {
            // Verificar acceso
            var hasAccess = await _context.ChatRooms
                .AnyAsync(c => c.Id == request.ChatRoomId &&
                    (c.DoctorId == request.UserId || c.PatientId == request.UserId));

            if (!hasAccess)
                throw new UnauthorizedAccessException("No tienes acceso a esta sala de chat");

            var messages = await _context.ChatMessages
                .Where(m => m.ChatRoomId == request.ChatRoomId)
                .OrderByDescending(m => m.SentAt)
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            // Obtener info de usuarios únicos
            var senderIds = messages.Select(m => m.SenderId).Distinct();
            var users = await _userCache.GetUsersAsync(senderIds);

            var result = messages.Select(m =>
            {
                users.TryGetValue(m.SenderId, out var sender);

                return new ChatMessageRequest
                (
                    m.Id,
                    m.ChatRoomId,
                    m.SenderId,
                    sender?.Name ?? "Usuario",
                    //SenderImageUrl = sender?.ProfileImageUrl,
                    m.Message,
                    m.SentAt,
                    m.IsRead
                );
            })
            .OrderBy(m => m.SentAt)
            .ToList();

            return result;
        }
    }
}
