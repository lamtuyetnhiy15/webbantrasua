using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTraSua.Models;

[Table("NewsArticles")]
public class NewsArticle
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Tiêu đề bài viết không được để trống")]
    [StringLength(250)]
    public string Title { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Slug { get; set; }

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = "Tin Tức";

    [StringLength(50)]
    public string? Badge { get; set; }

    [Required(ErrorMessage = "Tóm tắt bài viết không được để trống")]
    [StringLength(1000)]
    public string Summary { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nội dung bài viết không được để trống")]
    public string Content { get; set; } = string.Empty;

    [StringLength(300)]
    public string ImageUrl { get; set; } = string.Empty;

    public DateTime PublishedAt { get; set; } = DateTime.Now;

    [NotMapped]
    public DateTime PublishedDate
    {
        get => PublishedAt;
        set => PublishedAt = value;
    }

    public bool IsFeatured { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public int ViewCount { get; set; } = 0;
}
