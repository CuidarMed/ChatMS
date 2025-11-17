using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record SendMessageRequest
    (
        int ChatRoomId,
        string Message,
        int SenderId,
        UserDto SenderInfo // Info del usuario desde el front
    );
}
