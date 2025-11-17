using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record CreateChatRoomRequest
    (
        int PatientId,
        int DoctorId,
        UserDto DoctorInfo, // Info del doctor desde el front
        UserDto PatientInfo // Info del paciente desde el front
    );
}
