using QuanLyThuVien.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLyThuVien.Services
{
    public interface IRatingService
    {
        Task<Rating> AddOrUpdateRatingAsync(int documentId, string userId, int score, string comment);
        Task<decimal> GetAverageRatingAsync(int documentId);
        Task<List<Rating>> GetDocumentRatingsAsync(int documentId, int page = 1, int pageSize = 10);
        Task<Rating> GetUserRatingAsync(int documentId, string userId);
        Task<int> GetTotalRatingsCountAsync(int documentId);
        Task UpdateDocumentRatingStats(int documentId);
        Task DeleteRatingAsync(int ratingId);
    }
}