using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace QuanLyThuVien.Models
{
    public class Rating
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        [Display(Name = "Điểm đánh giá")]
        public int Score { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        [Display(Name = "Nhận xét")]
        public string? Comment { get; set; }

        [Display(Name = "Ngày đánh giá")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        // Foreign keys
        public int DocumentId { get; set; }
        public string UserId { get; set; }

        // Navigation properties
        [ForeignKey("DocumentId")]
        public Document Document { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }
    }
}
