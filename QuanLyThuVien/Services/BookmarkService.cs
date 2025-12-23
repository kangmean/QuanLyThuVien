using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Data;
using QuanLyThuVien.Models;

namespace QuanLyThuVien.Services
{
    public class BookmarkService : IBookmarkService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookmarkService> _logger;

        public BookmarkService(ApplicationDbContext context, ILogger<BookmarkService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> AddBookmarkAsync(string userId, int documentId)
        {
            try
            {
                // Kiểm tra đã bookmark chưa
                var existing = await _context.Bookmarks
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.DocumentId == documentId);

                if (existing != null)
                    return false;

                var bookmark = new Bookmark
                {
                    UserId = userId,
                    DocumentId = documentId,
                    CreatedAt = DateTime.Now
                };

                _context.Bookmarks.Add(bookmark);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding bookmark");
                return false;
            }
        }

        public async Task<bool> RemoveBookmarkAsync(string userId, int documentId)
        {
            try
            {
                var bookmark = await _context.Bookmarks
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.DocumentId == documentId);

                if (bookmark == null)
                    return false;

                _context.Bookmarks.Remove(bookmark);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing bookmark");
                return false;
            }
        }

        public async Task<bool> IsBookmarkedAsync(string userId, int documentId)
        {
            return await _context.Bookmarks
                .AnyAsync(b => b.UserId == userId && b.DocumentId == documentId);
        }

        public async Task<IEnumerable<Document>> GetUserBookmarksAsync(string userId, int page = 1, int pageSize = 20)
        {
            return await _context.Bookmarks
                .Where(b => b.UserId == userId)
                .Include(b => b.Document)
                    .ThenInclude(d => d.University)
                .Include(b => b.Document)
                    .ThenInclude(d => d.Subject)
                .Include(b => b.Document)
                    .ThenInclude(d => d.User)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => b.Document)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetBookmarkCountAsync(string userId)
        {
            return await _context.Bookmarks
                .CountAsync(b => b.UserId == userId);
        }
    }
}