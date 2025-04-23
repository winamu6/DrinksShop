using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using drinks.Models.Entities;

namespace drinks.Models.ViewModel
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [ForeignKey("Brand")]
        public int BrandId { get; set; }
        public virtual Brand? Brand { get; set; }

        [Range(0, 1000)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
        public string ImageUrl { get; set; } = "/images/products/default.png";
    }
}
