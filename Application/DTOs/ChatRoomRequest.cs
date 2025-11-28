using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record ChatRoomRequest
    (
        int Id,
        int PatientId,
        string PatientName,
        int DoctorId,
        string DoctorName,
        int AppointmentId, // ID de la consulta asociada (REQUERIDO - no nullable)
        DateTime CreatedAt,
        string LastMessage,
        DateTime? LastMessageTime,
        int UnreadCount,
        int? LastSenderId = null // ID del usuario que envió el último mensaje (userId de autenticación)
    );
}
