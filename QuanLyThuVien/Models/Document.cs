using System;
using System.Collections.Generic;
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

        [StringLength(1000, ErrorMessage = "Mô tả không quá 1000 ký tự")] // Tăng lên 1000
        [DataType(DataType.MultilineText)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Đường dẫn file")]
        public string FilePath { get; set; }

        [Required]
        [Display(Name = "Tên file gốc")]
        public string OriginalFileName { get; set; }

        [Display(Name = "Loại file")]
        public string FileType { get; set; }

        [Display(Name = "Kích thước")]
        [DisplayFormat(DataFormatString = "{0:N0} bytes")]
        public long FileSize { get; set; }

        [Display(Name = "Số trang")]
        public int? PageCount { get; set; }

        [Display(Name = "Ngày upload")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime UploadDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? LastUpdated { get; set; }

        // Foreign keys
        [Required]
        public string UserId { get; set; }

        public int? UniversityId { get; set; }
        public int? SubjectId { get; set; }

        // Rating properties
        [Display(Name = "Điểm đánh giá")]
        [Column(TypeName = "decimal(3,1)")]
        [Range(0, 5.0)]
        public decimal AverageRating { get; set; } = 0;

        [Display(Name = "Số lượt đánh giá")]
        public int RatingCount { get; set; } = 0;

        // Thống kê
        [Display(Name = "Lượt xem")]
        public int ViewCount { get; set; } = 0;

        [Display(Name = "Lượt tải")]
        public int DownloadCount { get; set; } = 0;

        // Trạng thái
        [Display(Name = "Trạng thái")]
        public DocumentStatus Status { get; set; } = DocumentStatus.Approved;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; }

        [ForeignKey("UniversityId")]
        public virtual University University { get; set; }

        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; }

        public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

        // Có thể thêm Tags/Categories sau
        // public virtual ICollection<DocumentTag> DocumentTags { get; set; }
    }

    public enum DocumentStatus
    {
        [Display(Name = "Chờ duyệt")]
        Pending,

        [Display(Name = "Đã duyệt")]
        Approved,

        [Display(Name = "Từ chối")]
        Rejected,

        [Display(Name = "Đã ẩn")]
        Hidden
    }
}