using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTraSua.Models;

[Table("CraftingSteps")]
public class CraftingStep
{
    [Key]
    public int Id { get; set; }

    public int StepNumber { get; set; }

    [Required]
    [StringLength(150)]
    public string StepName { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string IconCss { get; set; } = "bi bi-cup-hot";

    public bool IsActive { get; set; } = true;
}
