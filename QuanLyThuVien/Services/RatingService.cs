using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Data;
using QuanLyThuVien.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyThuVien.Services
{
    public class RatingService : IRatingService
    {
        private readonly ApplicationDbContext _context;

        public RatingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Rating> AddOrUpdateRatingAsync(int documentId, string userId, int score, string comment)
        {
            // Validate score
            if (score < 1 || score > 5)
                throw new ArgumentException("Điểm đánh giá phải từ 1 đến 5");

            // Check if user already rated this document
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.DocumentId == documentId && r.UserId == userId);

            if (existingRating != null)
            {
                // Update existing rating
                existingRating.Score = score;
                existingRating.Comment = comment;
                existingRating.CreatedAt = DateTime.Now;
            }
            else
            {
                // Create new rating
                existingRating = new Rating
                {
                    DocumentId = documentId,
                    UserId = userId,
                    Score = score,
                    Comment = comment,
                    CreatedAt = DateTime.Now
                };
                _context.Ratings.Add(existingRating);
            }

            // Recalculate average rating
            await UpdateDocumentRatingStats(documentId);

            await _context.SaveChangesAsync();
            return existingRating;
        }

        public async Task UpdateDocumentRatingStats(int documentId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.DocumentId == documentId)
                .ToListAsync();

            if (ratings.Any())
            {
                var document = await _context.Documents.FindAsync(documentId);
                if (document != null)
                {
                    document.AverageRating = (decimal)Math.Round(ratings.Average(r => r.Score), 1);
                    document.RatingCount = ratings.Count;
                }
            }
        }

        public async Task<decimal> GetAverageRatingAsync(int documentId)
        {
            var avg = await _context.Ratings
                .Where(r => r.DocumentId == documentId)
                .AverageAsync(r => (decimal?)r.Score);

            return avg ?? 0;
        }

        public async Task<List<Rating>> GetDocumentRatingsAsync(int documentId, int page = 1, int pageSize = 10)
        {
            return await _context.Ratings
                .Where(r => r.DocumentId == documentId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Rating> GetUserRatingAsync(int documentId, string userId)
        {
            return await _context.Ratings
                .FirstOrDefaultAsync(r => r.DocumentId == documentId && r.UserId == userId);
        }

        public async Task<int> GetTotalRatingsCountAsync(int documentId)
        {
            return await _context.Ratings
                .CountAsync(r => r.DocumentId == documentId);
        }

        public async Task DeleteRatingAsync(int ratingId)
        {
            var rating = await _context.Ratings.FindAsync(ratingId);
            if (rating != null)
            {
                _context.Ratings.Remove(rating);
                await _context.SaveChangesAsync();

                // Recalculate stats
                await UpdateDocumentRatingStats(rating.DocumentId);
            }
        }
    }
}