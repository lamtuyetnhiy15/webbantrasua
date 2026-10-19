using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTraSua.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("UserId")]
        public int Id { get; set; }

        [NotMapped]
        public int UserId
        {
            get => Id;
            set => Id = value;
        }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100, ErrorMessage = "Họ và tên không được quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được quá 100 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(255)]
        [Display(Name = "Mật khẩu")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Vai trò")]
        public string Role { get; set; } = "Customer"; // "Admin" hoặc "Customer"

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Các thuộc tính phụ trợ để duy trì tương thích ngược
        public string? Username { get; set; }
        public string? Password { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
