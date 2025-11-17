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
    public class GetUserChatRoomsQuery : IGetUserChatRoomsQuery
    {
        private readonly AppDbContext _context;
        private readonly IUserCacheService _userCache;

        public GetUserChatRoomsQuery(AppDbContext context, IUserCacheService userCache)
        {
            _context = context;
            _userCache = userCache;
        }

        public async Task<List<ChatRoomRequest>> ExecuteAsync(int userId)
        {
            var chatRooms = await _context.ChatRooms
                .Include(c => c.Messages)
                .Where(c => c.DoctorId == userId || c.PatientId == userId)
                .ToListAsync();

            // Obtener todos los IDs de usuarios únicos
            var userIds = chatRooms.SelectMany(c => new[] { c.DoctorId, c.PatientId }).Distinct();
            var users = await _userCache.GetUsersAsync(userIds);

            var result = chatRooms.Select(chatRoom =>
            {
                var lastMessage = chatRoom.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
                var unreadCount = chatRoom.Messages.Count(m => m.SenderId != userId && !m.IsRead);

                users.TryGetValue(chatRoom.DoctorId, out var doctor);
                users.TryGetValue(chatRoom.PatientId, out var patient);

                return new ChatRoomRequest
                (
                    chatRoom.Id,
                    chatRoom.DoctorId,
                    doctor?.Name ?? "Doctor",
                    //DoctorImageUrl = doctor?.ProfileImageUrl,
                    chatRoom.PatientId,
                    patient?.Name ?? "Paciente",
                    //PatientImageUrl = patient?.ProfileImageUrl,
                    chatRoom.CreatedAt,
                    lastMessage?.Message,
                    lastMessage?.SentAt,
                    unreadCount
                );
            })
            .OrderByDescending(c => c.LastMessageTime ?? c.CreatedAt)
            .ToList();

            return result;
        }
    }
}
