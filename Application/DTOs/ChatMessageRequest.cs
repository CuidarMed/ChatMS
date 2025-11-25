using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record ChatMessageRequest
    (
        int Id,
        int ChatRoomId,
        int SenderId,
        string SenderRole,
        string SenderName, // Analizar si ponemos el nombre del usuario que envia el mensaje
        string Message,
        DateTime SentAt,
        bool IsRead // Ver si emplementamos este apartado
    );
}
