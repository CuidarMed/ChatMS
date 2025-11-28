using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CreateChatRoomRequest
    {
        [JsonPropertyName("PatientId")]
        public int PatientId { get; set; }
        
        [JsonPropertyName("DoctorId")]
        public int DoctorId { get; set; }
        
        [JsonPropertyName("AppointmentId")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "AppointmentId must be a positive number.")]
        public int AppointmentId { get; set; } // REQUERIDO - no nullable
    }
}
