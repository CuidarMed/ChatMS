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
        DateTime CreatedAt,
        string LastMessage,
        DateTime? LastMessageTime,
        int UnreadCount
    );
}
