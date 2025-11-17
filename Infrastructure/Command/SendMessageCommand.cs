using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Command
{
    public class SendMessageCommand : ISendMessageCommand
    {
        private readonly AppDbContext _context;
        private readonly IUserCacheService _userCache;

        public SendMessageCommand(AppDbContext context, IUserCacheService userCache)
        {
            _context = context;
            _userCache = userCache;
        }

        public async Task<ChatMessage> ExecuteAsync(SendMessageRequest dto)
        {
            // Guardar info del usuario en caché
            if (dto.SenderInfo != null)
                await _userCache.SetUserAsync(dto.SenderId, dto.SenderInfo);

            // Verificar que la sala existe y el usuario pertenece a ella
            var chatRoom = await _context.ChatRooms
                .FirstOrDefaultAsync(c => c.Id == dto.ChatRoomId &&
                    (c.DoctorId == dto.SenderId || c.PatientId == dto.SenderId));

            if (chatRoom == null)
                throw new UnauthorizedAccessException("No tienes acceso a esta sala de chat");

            var chatMessage = new ChatMessage
            {
                ChatRoomId = dto.ChatRoomId,
                SenderId = dto.SenderId,
                Message = dto.Message,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            return chatMessage;
        }
    }
}
