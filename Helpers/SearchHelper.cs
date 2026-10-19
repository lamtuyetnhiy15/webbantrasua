using System.Globalization;
using System.Text;

namespace WebTraSua.Helpers;

public static class SearchHelper
{
    /// <summary>
    /// Xóa dấu tiếng Việt và chuẩn hóa về chữ thường (ví dụ: 'Trà Sữa Thái Đỏ' -> 'tra sua thai do')
    /// </summary>
    public static string RemoveDiacritics(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace('đ', 'd')
            .Replace('Đ', 'd')
            .Trim()
            .ToLowerInvariant();
    }

    /// <summary>
    /// Kiểm tra chuỗi target có khớp với từ khóa tìm kiếm (hỗ trợ cả có dấu, không dấu, viết hoa/thường, và từng từ khóa)
    /// </summary>
    public static bool MatchesSearch(string? target, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(target) || string.IsNullOrWhiteSpace(searchTerm))
            return false;

        var trimmedSearch = searchTerm.Trim();

        // 1. So khớp trực tiếp không phân biệt hoa thường có dấu (xử lý chuẩn Unicode C# cho Đ/đ, Ơ/ơ...)
        if (target.Contains(trimmedSearch, StringComparison.CurrentCultureIgnoreCase) ||
            target.Contains(trimmedSearch, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // 2. So khớp không dấu (vd: 'tra sua thai do' khớp 'Trà Sữa Thái Đỏ', hoặc 'trà sữa thái đỏ' khớp 'Trà Sữa Thái Đỏ')
        var targetClean = RemoveDiacritics(target);
        var searchClean = RemoveDiacritics(trimmedSearch);

        if (targetClean.Contains(searchClean, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // 3. So khớp tất cả các từ đơn lẻ (vd: 'thái đỏ' khớp 'Trà Sữa Thái Đỏ')
        var searchTokens = searchClean.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (searchTokens.Length > 1 && searchTokens.All(token => targetClean.Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return false;
    }
}
