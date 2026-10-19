using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTraSua.Models;

public class Product
{
    [Key]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public string Description { get; set; } = string.Empty;

    [StringLength(250)]
    public string ImageUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn danh mục cho sản phẩm")]
    [StringLength(100)]
    [Display(Name = "Danh mục")]
    public string Category { get; set; } = "Trà Sữa Đậm Hương";

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
