using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyThuVien.Data;
using QuanLyThuVien.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace QuanLyThuVien.Controllers
{
    [Authorize]
    public class RatingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RatingsController> _logger;

        public RatingsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<RatingsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Ratings/GetRatings - THÊM ACTION NÀY
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetRatings(int documentId, int page = 1)
        {
            try
            {
                const int pageSize = 5;

                // Lấy ratings với pagination
                var ratings = await _context.Ratings
                    .Where(r => r.DocumentId == documentId)
                    .Include(r => r.User)
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var totalCount = await _context.Ratings
                    .CountAsync(r => r.DocumentId == documentId);

                var hasMore = totalCount > page * pageSize;

                // Tính average rating
                var averageRating = 0m;
                if (totalCount > 0)
                {
                    var avg = await _context.Ratings
                        .Where(r => r.DocumentId == documentId)
                        .AverageAsync(r => (decimal?)r.Score);
                    averageRating = Math.Round(avg ?? 0, 1);
                }

                // Lấy user rating hiện tại nếu đã đăng nhập
                Rating userRating = null;
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    userRating = await _context.Ratings
                        .FirstOrDefaultAsync(r => r.DocumentId == documentId && r.UserId == userId);
                }

                return Json(new
                {
                    success = true,
                    ratings = ratings.Select(r => new
                    {
                        id = r.Id,
                        userName = r.User?.UserName ?? "Ẩn danh",
                        score = r.Score,
                        comment = r.Comment,
                        createdAt = r.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                        isCurrentUser = User.Identity.IsAuthenticated &&
                                       r.UserId == User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    }),
                    hasMore,
                    averageRating,
                    totalCount,
                    userRating = userRating != null ? new
                    {
                        score = userRating.Score,
                        comment = userRating.Comment
                    } : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting ratings for document {documentId}");
                return Json(new
                {
                    success = false,
                    message = "Không thể tải đánh giá"
                });
            }
        }

        // POST: Ratings/Create - SỬA ACTION NÀY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int documentId, int score, string comment)
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

                TempData["SuccessMessage"] = "Đã lưu đánh giá của bạn!";
                return RedirectToAction("Details", "Documents", new { id = documentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating rating for document {documentId}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu đánh giá";
                return RedirectToAction("Details", "Documents", new { id = documentId });
            }
        }

        // POST: Ratings/Delete - SỬA ACTION NÀY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var rating = await _context.Ratings
                    .Include(r => r.Document)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (rating == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đánh giá" });
                }

                var documentId = rating.DocumentId;
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Chỉ cho phép xóa rating của chính user
                if (rating.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Json(new { success = false, message = "Không có quyền xóa" });
                }

                _context.Ratings.Remove(rating);
                await _context.SaveChangesAsync();

                // Update document average rating
                await UpdateDocumentRating(documentId);

                return Json(new
                {
                    success = true,
                    message = "Đã xóa đánh giá",
                    documentId = documentId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting rating {id}");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        // THÊM ACTION MỚI: SubmitRating (cho AJAX)
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
                    return Json(new { success = false, message = "Điểm đánh giá phải từ 1 đến 5" });
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

                // Lấy thông tin mới
                var newAverage = await _context.Documents
                    .Where(d => d.Id == documentId)
                    .Select(d => new { d.AverageRating, d.RatingCount })
                    .FirstOrDefaultAsync();

                return Json(new
                {
                    success = true,
                    message = "Đã lưu đánh giá",
                    averageRating = newAverage?.AverageRating ?? 0,
                    ratingCount = newAverage?.RatingCount ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error submitting rating for document {documentId}");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        private async Task UpdateDocumentRating(int documentId)
        {
            try
            {
                var ratings = await _context.Ratings
                    .Where(r => r.DocumentId == documentId)
                    .ToListAsync();

                var document = await _context.Documents.FindAsync(documentId);
                if (document == null) return;

                if (ratings.Any())
                {
                    var averageRating = ratings.Average(r => r.Score);
                    document.AverageRating = (decimal)Math.Round(averageRating, 1);
                    document.RatingCount = ratings.Count;
                }
                else
                {
                    document.AverageRating = 0;
                    document.RatingCount = 0;
                }

                _context.Update(document);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating rating for document {documentId}");
            }
        }
    }
}