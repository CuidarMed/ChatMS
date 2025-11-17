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
    public class CreateChatRoomCommand : ICreateChatRoomCommand
    {
        private readonly AppDbContext _context;
        private readonly IUserCacheService _userCache;

        public CreateChatRoomCommand(AppDbContext context, IUserCacheService userCache)
        {
            _context = context;
            _userCache = userCache;
        }

        public async Task<ChatRoom> ExecuteAsync(CreateChatRoomRequest dto)
        {
            // Guardar info de usuarios en caché
            if (dto.DoctorInfo != null)
                await _userCache.SetUserAsync(dto.DoctorId, dto.DoctorInfo);

            if (dto.PatientInfo != null)
                await _userCache.SetUserAsync(dto.PatientId, dto.PatientInfo);

            // Verificar si ya existe una sala entre estos usuarios
            var existingRoom = await _context.ChatRooms
                .FirstOrDefaultAsync(c =>
                    (c.DoctorId == dto.DoctorId && c.PatientId == dto.PatientId) ||
                    (c.DoctorId == dto.PatientId && c.PatientId == dto.DoctorId));

            if (existingRoom != null)
                return existingRoom;

            var chatRoom = new ChatRoom
            {
                DoctorId = dto.DoctorId,
                PatientId = dto.PatientId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.ChatRooms.Add(chatRoom);
            await _context.SaveChangesAsync();

            return chatRoom;
        }
    }
}
