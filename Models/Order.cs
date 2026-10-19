using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTraSua.Models;

public class Order
{
    [Key]
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.Now;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
