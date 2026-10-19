using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTraSua.Models;

[Table("BrandStoryMilestones")]
public class BrandStoryMilestone
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(10)]
    public string Year { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public int OrderIndex { get; set; }

    public bool IsActive { get; set; } = true;
}
