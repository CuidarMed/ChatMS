using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ChatRoom
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int PatientId { get; set; }
        [Required]
        public int DoctorID { get; set; }
        [Required]
        public int AppointmentId { get; set; } // ID de la consulta asociada (REQUERIDO - no nullable)
        [Required]
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        
        // Navigation properties
        public User? Doctor { get; set; }
        public User? Patient { get; set; }
    }
}
