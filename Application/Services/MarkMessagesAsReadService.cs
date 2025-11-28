using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class MarkMessagesAsReadService : IMarkMessagesAsReadService
    {
        private readonly IMarkMessagesAsReadCommand markMessagesAsReadCommand;

        public MarkMessagesAsReadService(IMarkMessagesAsReadCommand markMessagesAsReadCommand)
        {
            this.markMessagesAsReadCommand = markMessagesAsReadCommand;
        }
        public async Task MarkMessagesAsReadAsync(int chatRoomId, int userId, string userRole)
        {
            await markMessagesAsReadCommand.ExecuteAsync(chatRoomId, userId, userRole);
        }
    }
}
