using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class SendMessageRequest
    {
        public int ChatRoomId { get; set; }
        public string Message { get; set; }
        public int SenderId { get; set; }
        public UserDto SenderInfo { get; set; } // Info del usuario desde el front
    }
}
