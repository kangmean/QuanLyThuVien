using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Data;
using QuanLyThuVien.Models;
using QuanLyThuVien.Services;
using System.Security.Claims;

namespace QuanLyThuVien.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookmarkService _bookmarkService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            ApplicationDbContext context,
            IBookmarkService bookmarkService,
            ILogger<DashboardController> logger)
        {
            _context = context;
            _bookmarkService = bookmarkService;
            _logger = logger;
        }

        // GET: Dashboard/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var model = await GetDashboardDataAsync(userId);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["ErrorMessage"] = "Không thể tải thông tin dashboard";
                return View(new DashboardViewModel());
            }
        }

        private async Task<DashboardViewModel> GetDashboardDataAsync(string userId)
        {
            var model = new DashboardViewModel();

            // 1. Personal Statistics
            model.PersonalStats = await GetPersonalStatisticsAsync(userId);

            // 2. Recent Uploads (5 documents)
            model.RecentUploads = await _context.Documents
                .Where(d => d.UserId == userId)
                .Include(d => d.University)
                .Include(d => d.Subject)
                .OrderByDescending(d => d.UploadDate)
                .Take(5)
                .ToListAsync();

            // 3. Recent Ratings (5 ratings)
            model.RecentRatings = await _context.Ratings
                .Where(r => r.UserId == userId)
                .Include(r => r.Document)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            // 4. Recent Bookmarks (5 bookmarks)
            model.RecentBookmarks = await _context.Bookmarks
                .Where(b => b.UserId == userId)
                .Include(b => b.Document)
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .Select(b => b.Document)
                .ToListAsync();

            return model;
        }

        private async Task<PersonalStatistics> GetPersonalStatisticsAsync(string userId)
        {
            var stats = new PersonalStatistics();

            // Total documents uploaded
            stats.TotalDocuments = await _context.Documents
                .CountAsync(d => d.UserId == userId);

            // Total views/downloads
            var documents = await _context.Documents
                .Where(d => d.UserId == userId)
                .ToListAsync();

            stats.TotalViews = documents.Sum(d => d.ViewCount);
            stats.TotalDownloads = documents.Sum(d => d.DownloadCount);

            // Average rating
            var ratings = await _context.Ratings
                .Where(r => r.UserId == userId)
                .ToListAsync();

            stats.AverageRating = ratings.Any()
                ? Math.Round(ratings.Average(r => r.Score), 1)
                : 0;

            // Total ratings given
            stats.TotalRatings = ratings.Count;

            // Total bookmarks
            stats.TotalBookmarks = await _bookmarkService.GetBookmarkCountAsync(userId);

            // Popularity score
            stats.PopularityScore = (stats.TotalViews * 1) +
                                   (stats.TotalDownloads * 2) +
                                   (stats.TotalRatings * 5);

            // Documents per day
            var firstUpload = await _context.Documents
                .Where(d => d.UserId == userId)
                .OrderBy(d => d.UploadDate)
                .Select(d => d.UploadDate)
                .FirstOrDefaultAsync();

            if (firstUpload != default)
            {
                var daysSinceFirstUpload = (DateTime.Now - firstUpload).TotalDays;
                stats.DocumentsPerDay = daysSinceFirstUpload > 0
                    ? Math.Round(stats.TotalDocuments / daysSinceFirstUpload, 2)
                    : stats.TotalDocuments;
            }

            return stats;
        }
    }

    // ViewModel classes
    public class DashboardViewModel
    {
        public PersonalStatistics PersonalStats { get; set; } = new PersonalStatistics();
        public List<Document> RecentUploads { get; set; } = new List<Document>();
        public List<Rating> RecentRatings { get; set; } = new List<Rating>();
        public List<Document> RecentBookmarks { get; set; } = new List<Document>();
    }

    public class PersonalStatistics
    {
        public int TotalDocuments { get; set; }
        public int TotalViews { get; set; }
        public int TotalDownloads { get; set; }
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public int TotalBookmarks { get; set; }
        public int PopularityScore { get; set; }
        public double DocumentsPerDay { get; set; }
    }
}