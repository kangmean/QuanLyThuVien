using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        public RatingsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: Ratings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int documentId, int score, string comment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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

            return RedirectToAction("Details", "Documents", new { id = documentId });
        }

        // POST: Ratings/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var rating = await _context.Ratings.FindAsync(id);
            if (rating == null)
            {
                return NotFound();
            }

            var documentId = rating.DocumentId;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Chỉ cho phép xóa rating của chính user
            if (rating.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();

            // Update document average rating
            await UpdateDocumentRating(documentId);

            return RedirectToAction("Details", "Documents", new { id = documentId });
        }

        private async Task UpdateDocumentRating(int documentId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.DocumentId == documentId)
                .ToListAsync();

            if (ratings.Any())
            {
                var averageRating = ratings.Average(r => r.Score);
                var ratingCount = ratings.Count;

                var document = await _context.Documents.FindAsync(documentId);
                if (document != null)
                {
                    document.AverageRating = Math.Round(averageRating, 1);
                    document.RatingCount = ratingCount;
                    _context.Update(document);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                var document = await _context.Documents.FindAsync(documentId);
                if (document != null)
                {
                    document.AverageRating = 0;
                    document.RatingCount = 0;
                    _context.Update(document);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}