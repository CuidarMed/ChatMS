using Application.Interfaces;
using Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var messages = await _context.ChatMessages
                .Where(m => m.ChatRoomId == chatRoomId &&
                           m.SenderId != userId &&
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
