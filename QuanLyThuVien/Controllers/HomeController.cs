using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace QuanLyThuVien.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // LẤY 6 DOCUMENTS MỚI NHẤT
            var recentDocuments = await _context.Documents
                .OrderByDescending(d => d.UploadDate)
                .Take(6)
                .ToListAsync();

            return View(recentDocuments);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Search(string searchString, int? universityId, int? subjectId, string fileType, int page = 1, int pageSize = 9)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["UniversityFilter"] = universityId;
            ViewData["SubjectFilter"] = subjectId;
            ViewData["FileTypeFilter"] = fileType;

            var documents = _context.Documents
                .Include(d => d.University)
                .Include(d => d.Subject)
                .Include(d => d.User)
                .AsQueryable();

            // Áp dụng search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                documents = documents.Where(d =>
                    d.Title.Contains(searchString) ||
                    d.Description.Contains(searchString));
            }

            // Áp dụng university filter
            if (universityId.HasValue)
            {
                documents = documents.Where(d => d.UniversityId == universityId);
            }

            // Áp dụng subject filter
            if (subjectId.HasValue)
            {
                documents = documents.Where(d => d.SubjectId == subjectId);
            }

            // Áp dụng file type filter
            if (!string.IsNullOrEmpty(fileType))
            {
                documents = documents.Where(d => d.FileType == fileType);
            }

            // Sắp xếp và phân trang
            var totalCount = await documents.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var results = await documents
                .OrderByDescending(d => d.UploadDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền dữ liệu filter cho view
            ViewData["Universities"] = await _context.Universities.ToListAsync();
            ViewData["Subjects"] = await _context.Subjects.ToListAsync();
            ViewData["FileTypes"] = new List<string> { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xlsx", ".txt" };

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalCount"] = totalCount;

            return View(results);
        }
    }
}