using QuanLyThuVien.Models;

namespace QuanLyThuVien.Services
{
    public interface IBookmarkService
    {
        Task<bool> AddBookmarkAsync(string userId, int documentId);
        Task<bool> RemoveBookmarkAsync(string userId, int documentId);
        Task<bool> IsBookmarkedAsync(string userId, int documentId);
        Task<IEnumerable<Document>> GetUserBookmarksAsync(string userId, int page = 1, int pageSize = 20);
        Task<int> GetBookmarkCountAsync(string userId);
    }
}