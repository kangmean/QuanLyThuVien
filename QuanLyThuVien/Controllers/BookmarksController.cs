using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyThuVien.Services;
using System.Security.Claims;
using QuanLyThuVien.Models;

namespace QuanLyThuVien.Controllers
{
    [Authorize]
    public class BookmarksController : Controller
    {
        private readonly IBookmarkService _bookmarkService;
        private readonly ILogger<BookmarksController> _logger;

        public BookmarksController(IBookmarkService bookmarkService, ILogger<BookmarksController> logger)
        {
            _bookmarkService = bookmarkService;
            _logger = logger;
        }

        // GET: Bookmarks/Index
        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var bookmarks = await _bookmarkService.GetUserBookmarksAsync(userId, page, 20);
                var totalCount = await _bookmarkService.GetBookmarkCountAsync(userId);

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling(totalCount / 20.0);
                ViewBag.TotalCount = totalCount;

                return View(bookmarks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading bookmarks");
                TempData["ErrorMessage"] = "Không thể tải danh sách bookmark";
                return View(new List<Document>());
            }
        }

        // AJAX: Toggle bookmark
        [HttpPost]
        public async Task<IActionResult> Toggle(int documentId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "Chưa đăng nhập" });

                var isBookmarked = await _bookmarkService.IsBookmarkedAsync(userId, documentId);

                if (isBookmarked)
                {
                    // Remove bookmark
                    var result = await _bookmarkService.RemoveBookmarkAsync(userId, documentId);
                    return Json(new
                    {
                        success = result,
                        message = "Đã xóa khỏi bookmark",
                        isBookmarked = false
                    });
                }
                else
                {
                    // Add bookmark
                    var result = await _bookmarkService.AddBookmarkAsync(userId, documentId);
                    return Json(new
                    {
                        success = result,
                        message = "Đã thêm vào bookmark",
                        isBookmarked = true
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling bookmark");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        // AJAX: Check bookmark status
        [HttpGet]
        public async Task<IActionResult> Check(int documentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Json(new { isBookmarked = false });

            var isBookmarked = await _bookmarkService.IsBookmarkedAsync(userId, documentId);
            return Json(new { isBookmarked });
        }
    }
}