namespace WebTraSua.Models;

public class BrandStoryViewModel
{
    public string HeroBadge { get; set; } = "Triết Lý Thương Hiệu";
    public string HeroTitle { get; set; } = "Chuyện Của TN Tea";
    public string HeroSubtitle { get; set; } = "Khơi nguồn cảm hứng từ các loài hoa tinh khiết, kết hợp cùng vị dịu nhẹ của trà sữa tươi lá cao cấp, mang đến sự thư giãn và tìm lại bình yên giữa bộn bề cuộc sống.";

    public List<BrandStoryMilestone> Milestones { get; set; } = new();
    public List<CraftingStep> CraftingSteps { get; set; } = new();
}
