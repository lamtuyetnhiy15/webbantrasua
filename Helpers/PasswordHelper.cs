using System.Security.Cryptography;
using System.Text;

namespace WebTraSua.Helpers
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Băm mật khẩu sử dụng SHA-256
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Xác thực mật khẩu: Hỗ trợ cả mật khẩu băm SHA-256 và chuỗi thô (plain-text) cho tài khoản seed
        /// </summary>
        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedHash))
                return false;

            // Kiểm tra khớp trực tiếp (chuỗi thuần)
            if (inputPassword == storedHash)
                return true;

            // Kiểm tra khớp SHA-256
            var hashedInput = HashPassword(inputPassword);
            return hashedInput == storedHash;
        }
    }
}
