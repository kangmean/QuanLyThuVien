using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Data;
using QuanLyThuVien.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;

namespace QuanLyThuVien.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DocumentsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Documents
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Documents.Include(d => d.Subject).Include(d => d.University).Include(d => d.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Documents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .Include(d => d.Subject)
                .Include(d => d.University)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // GET: Documents/Create
        public IActionResult Create()
        {
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name");
            ViewData["UniversityId"] = new SelectList(_context.Universities, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Documents/Create - ĐÃ ĐƯỢC CẬP NHẬT
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,UniversityId,SubjectId")] Document document, IFormFile file)
        {
            Console.WriteLine("=== DEBUG UPLOAD ===");
            Console.WriteLine($"File is null: {file == null}");
            Console.WriteLine($"File length: {file?.Length}");

            // TẠM THỜI BỎ CHECK ModelState.IsValid - CHỈ CHECK FILE
            if (file != null && file.Length > 0)
            {
                Console.WriteLine("File validation passed");

                // Kiểm tra file size (max 10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "File không được vượt quá 10MB");
                    ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name", document.SubjectId);
                    ViewData["UniversityId"] = new SelectList(_context.Universities, "Id", "Name", document.UniversityId);
                    return View(document);
                }

                // Kiểm tra file type
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xlsx", ".txt" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("", "Chỉ chấp nhận file PDF, Word, Excel, PowerPoint, Text");
                    ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name", document.SubjectId);
                    ViewData["UniversityId"] = new SelectList(_context.Universities, "Id", "Name", document.UniversityId);
                    return View(document);
                }

                // Tạo tên file unique
                var fileName = Guid.NewGuid().ToString() + fileExtension;
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

                // Đảm bảo thư mục uploads tồn tại
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                Console.WriteLine($"File saved: {fileName}");
                Console.WriteLine($"File path: {filePath}");

                // Lưu thông tin document - QUAN TRỌNG: SET FilePath
                document.FilePath = fileName;
                document.FileType = fileExtension;
                document.FileSize = file.Length;
                document.UploadDate = DateTime.Now;
                document.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                Console.WriteLine($"Document FilePath: {document.FilePath}");
                Console.WriteLine($"Document FileType: {document.FileType}");

                _context.Add(document);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                Console.WriteLine("File is NULL or empty - cannot proceed");
            }

            if (file == null)
            {
                ModelState.AddModelError("", "Vui lòng chọn file để upload");
            }

            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name", document.SubjectId);
            ViewData["UniversityId"] = new SelectList(_context.Universities, "Id", "Name", document.UniversityId);
            return View(document);
        }

        // GET: Documents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name", document.SubjectId);
            ViewData["UniversityId"] = new SelectList(_context.Universities, "Id", "Name", document.UniversityId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", document.UserId);
            return View(document);
        }

        // POST: Documents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,FilePath,FileType,FileSize,UploadDate,UserId,UniversityId,SubjectId")] Document document)
        {
            if (id != document.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(document);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentExists(document.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name", document.SubjectId);
            ViewData["UniversityId"] = new SelectList(_context.Universities, "Id", "Name", document.UniversityId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", document.UserId);
            return View(document);
        }

        // GET: Documents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .Include(d => d.Subject)
                .Include(d => d.University)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document != null)
            {
                _context.Documents.Remove(document);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DOWNLOAD METHOD - ĐÃ ĐƯỢC THÊM
        [Authorize]
        public async Task<IActionResult> Download(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .Include(d => d.University)
                .Include(d => d.Subject)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            var path = Path.Combine(_environment.WebRootPath, "uploads", document.FilePath);
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            // Xác định content type
            var contentType = GetContentType(document.FileType);
            var safeFileName = document.Title + document.FileType;

            return File(memory, contentType, safeFileName);
        }

        private string GetContentType(string fileType)
        {
            var types = new Dictionary<string, string>
            {
                { ".pdf", "application/pdf" },
                { ".doc", "application/msword" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".ppt", "application/vnd.ms-powerpoint" },
                { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".txt", "text/plain" }
            };

            return types.ContainsKey(fileType.ToLower()) ? types[fileType.ToLower()] : "application/octet-stream";
        }

        private bool DocumentExists(int id)
        {
            return _context.Documents.Any(e => e.Id == id);
        }
    }
}