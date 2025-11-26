using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace QuanLyThuVien.Models
{
    public class Document
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(200, ErrorMessage = "Tiêu đề không quá 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Đường dẫn file")]
        public string FilePath { get; set; }

        [Display(Name = "Loại file")]
        public string FileType { get; set; }

        [Display(Name = "Kích thước")]
        public long FileSize { get; set; }

        [Display(Name = "Ngày upload")]
        public DateTime UploadDate { get; set; } = DateTime.Now;

        // Foreign key - SỬA THÀNH STRING
        public string UserId { get; set; }

        // Navigation property - SỬA THÀNH IdentityUser
        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }
    }
}