using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyThuVien.Data;
using QuanLyThuVien.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace QuanLyThuVien.Controllers
{
    [Authorize]
    public class RatingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RatingsController> _logger;

        public RatingsController(
            ApplicationDbContext context,
            ILogger<RatingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: Ratings/SubmitRating - GỬI ĐÁNH GIÁ (FORM SUBMIT)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRating(int documentId, int score, string comment)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Validate score
                if (score < 1 || score > 5)
                {
                    TempData["ErrorMessage"] = "Điểm đánh giá phải từ 1 đến 5";
                    return RedirectToAction("Details", "Documents", new { id = documentId });
                }

                // Kiểm tra user đã rating document này chưa
                var existingRating = await _context.Ratings
                    .FirstOrDefaultAsync(r => r.DocumentId == documentId && r.UserId == userId);

                if (existingRating != null)
                {
                    // Update rating cũ
                    existingRating.Score = score;
                    existingRating.Comment = comment;
                    existingRating.CreatedAt = DateTime.Now;
                    _context.Update(existingRating);
                }
                else
                {
                    // Tạo rating mới
                    var rating = new Rating
                    {
                        DocumentId = documentId,
                        UserId = userId,
                        Score = score,
                        Comment = comment,
                        CreatedAt = DateTime.Now
                    };
                    _context.Add(rating);
                }

                await _context.SaveChangesAsync();

                // Update document average rating
                await UpdateDocumentRating(documentId);

                TempData["SuccessMessage"] = "Đã lưu đánh giá thành công!";
                return RedirectToAction("Details", "Documents", new { id = documentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error submitting rating for document {documentId}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu đánh giá";
                return RedirectToAction("Details", "Documents", new { id = documentId });
            }
        }

        // POST: Ratings/Delete - XÓA ĐÁNH GIÁ (FORM SUBMIT)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var rating = await _context.Ratings.FindAsync(id);

                if (rating == null || rating.UserId != userId)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đánh giá hoặc không có quyền xóa";
                    return RedirectToAction("Details", "Documents", new { id = rating?.DocumentId ?? 0 });
                }

                var documentId = rating.DocumentId;
                _context.Ratings.Remove(rating);
                await _context.SaveChangesAsync();

                // Update document rating
                await UpdateDocumentRating(documentId);

                TempData["SuccessMessage"] = "Đã xóa đánh giá thành công";
                return RedirectToAction("Details", "Documents", new { id = documentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting rating {id}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa đánh giá";
                return RedirectToAction("Details", "Documents", new { id = 0 });
            }
        }

        private async Task UpdateDocumentRating(int documentId)
        {
            try
            {
                // Tính toán trực tiếp từ database
                var ratingStats = await _context.Ratings
                    .Where(r => r.DocumentId == documentId)
                    .GroupBy(r => r.DocumentId)
                    .Select(g => new
                    {
                        AverageRating = (decimal?)g.Average(r => (decimal)r.Score),
                        RatingCount = g.Count()
                    })
                    .FirstOrDefaultAsync();

                var document = await _context.Documents.FindAsync(documentId);
                if (document != null)
                {
                    if (ratingStats != null && ratingStats.RatingCount > 0)
                    {
                        document.AverageRating = Math.Round(ratingStats.AverageRating ?? 0, 1);
                        document.RatingCount = ratingStats.RatingCount;
                    }
                    else
                    {
                        document.AverageRating = 0;
                        document.RatingCount = 0;
                    }

                    document.LastUpdated = DateTime.Now;
                    _context.Update(document);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating document rating for document {documentId}");
            }
        }
    }
}