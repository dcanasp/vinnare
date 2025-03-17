using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class WishList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //[Column(TypeName = "serial")]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int ProductId { get; set; }

        // Navigation Property
        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("ProductId")]	
        public Product Product { get; set; }
    }
}
