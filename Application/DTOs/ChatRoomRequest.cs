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
        int DoctorId,
        string DoctorName,
        int PatientId,
        string PatientName,
        DateTime CreatedAt,
        string LastMessage,
        DateTime? LastMessageTime,
        int UnreadCount
    );
}
