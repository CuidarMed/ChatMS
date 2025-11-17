using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ChatMessage
    {
        [Key]
        public int Id {  get; set; }
        [Required]
        public int ChatRoomId {  get; set; }
        [Required]
        public int SenderId { get; set; }
        public string? Message { get; set; }
        [Required]
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; } // Ver si implementamos este apartado

        public virtual ChatRoom ChatRoom { get; set; }
    }
}
