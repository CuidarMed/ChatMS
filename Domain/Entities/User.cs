using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("UserId")]
        public int Id { get; set; }
        
        // Nota: Name y Email no se usan en ChatMS, solo se necesita el Id para las foreign keys
        // Estos campos están en la tabla Users compartida pero no los mapeamos aquí
    }
}

