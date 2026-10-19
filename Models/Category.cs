using System.ComponentModel.DataAnnotations;

namespace WebTraSua.Models;

public class Category
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên mục lục không được để trống")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Icon { get; set; } = "bi bi-tag-fill";
}
