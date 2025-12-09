using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyThuVien.Data;
using QuanLyThuVien.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyThuVien.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DocumentService> _logger;

        // Configuration
        private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".doc", ".pptx", ".ppt", ".txt" };
        private readonly long _maxFileSize = 50 * 1024 * 1024; // 50MB

        public DocumentService(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ILogger<DocumentService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        // ========== CRUD OPERATIONS ==========

        public async Task<Document> GetDocumentByIdAsync(int id)
        {
            try
            {
                return await _context.Documents
                    .Include(d => d.User)
                    .Include(d => d.University)
                    .Include(d => d.Subject)
                    .Include(d => d.Ratings)
                        .ThenInclude(r => r.User)
                    .FirstOrDefaultAsync(d => d.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting document by ID: {id}");
                throw;
            }
        }

        public async Task<List<Document>> GetAllDocumentsAsync()
        {
            try
            {
                return await _context.Documents
                    .Include(d => d.University)
                    .Include(d => d.Subject)
                    .Where(d => d.Status == DocumentStatus.Approved)
                    .OrderByDescending(d => d.UploadDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all documents");
                throw;
            }
        }

        public async Task<List<Document>> GetRecentDocumentsAsync(int count = 10)
        {
            try
            {
                return await _context.Documents
                    .Include(d => d.University)
                    .Include(d => d.Subject)
                    .Where(d => d.Status == DocumentStatus.Approved)
                    .OrderByDescending(d => d.UploadDate)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting recent documents: {count}");
                throw;
            }
        }

        public async Task<Document> CreateDocumentAsync(Document document, IFormFile file, string userId)
        {
            try
            {
                // Validate file
                ValidateFile(file);

                // Upload file
                var filePath = await UploadFileAsync(file);

                // Create document object
                var newDocument = new Document
                {
                    Title = document.Title,
                    Description = document.Description,
                    FilePath = filePath,
                    OriginalFileName = file.FileName,
                    FileType = Path.GetExtension(file.FileName).ToLower(),
                    FileSize = file.Length,
                    UserId = userId,
                    UniversityId = document.UniversityId,
                    SubjectId = document.SubjectId,
                    UploadDate = DateTime.Now,
                    Status = DocumentStatus.Approved, // Hoặc Pending tùy cấu hình
                    ViewCount = 0,
                    DownloadCount = 0,
                    AverageRating = 0,
                    RatingCount = 0
                };

                _context.Documents.Add(newDocument);
                await _context.SaveChangesAsync();

                return newDocument;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating document");
                throw;
            }
        }

        public async Task UpdateDocumentAsync(int id, Document document, IFormFile? file = null)
        {
            try
            {
                var existingDocument = await _context.Documents.FindAsync(id);
                if (existingDocument == null)
                    throw new Exception("Document not found");

                // Update basic info
                existingDocument.Title = document.Title;
                existingDocument.Description = document.Description;
                existingDocument.UniversityId = document.UniversityId;
                existingDocument.SubjectId = document.SubjectId;
                existingDocument.LastUpdated = DateTime.Now;

                // If new file provided
                if (file != null)
                {
                    ValidateFile(file);

                    // Delete old file
                    await DeleteFileAsync(existingDocument.FilePath);

                    // Upload new file
                    existingDocument.FilePath = await UploadFileAsync(file);
                    existingDocument.OriginalFileName = file.FileName;
                    existingDocument.FileType = Path.GetExtension(file.FileName).ToLower();
                    existingDocument.FileSize = file.Length;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating document: {id}");
                throw;
            }
        }

        public async Task DeleteDocumentAsync(int id)
        {
            try
            {
                var document = await _context.Documents.FindAsync(id);
                if (document == null)
                    throw new Exception("Document not found");

                // Delete file from storage
                await DeleteFileAsync(document.FilePath);

                // Delete from database
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting document: {id}");
                throw;
            }
        }

        // ========== FILE OPERATIONS ==========

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            try
            {
                // Create unique filename
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads/documents");
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Ensure directory exists
                Directory.CreateDirectory(uploadsFolder);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/uploads/documents/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file: {file.FileName}");
                throw new Exception("Không thể upload file");
            }
        }

        public async Task DeleteFileAsync(string filePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    var physicalPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
                    if (File.Exists(physicalPath))
                    {
                        File.Delete(physicalPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file: {filePath}");
                // Không throw để tránh ảnh hưởng đến các thao tác khác
            }
        }

        private void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("Vui lòng chọn file");

            if (file.Length > _maxFileSize)
                throw new Exception($"File quá lớn. Kích thước tối đa: {_maxFileSize / (1024 * 1024)}MB");

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(extension))
                throw new Exception($"Định dạng file không được hỗ trợ. Chấp nhận: {string.Join(", ", _allowedExtensions)}");
        }

        // ========== STATISTICS & TRACKING ==========

        public async Task IncrementViewCountAsync(int documentId)
        {
            try
            {
                var document = await _context.Documents.FindAsync(documentId);
                if (document != null)
                {
                    document.ViewCount++;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error incrementing view count for document: {documentId}");
            }
        }

        public async Task IncrementDownloadCountAsync(int documentId)
        {
            try
            {
                var document = await _context.Documents.FindAsync(documentId);
                if (document != null)
                {
                    document.DownloadCount++;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error incrementing download count for document: {documentId}");
            }
        }

        public async Task UpdateDocumentStatusAsync(int documentId, DocumentStatus status)
        {
            try
            {
                var document = await _context.Documents.FindAsync(documentId);
                if (document != null)
                {
                    document.Status = status;
                    document.LastUpdated = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status for document: {documentId}");
                throw;
            }
        }

        // ========== SEARCH & FILTER ==========

        public async Task<List<Document>> SearchDocumentsAsync(
            string keyword,
            int? universityId,
            int? subjectId,
            string fileType = null,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                var query = _context.Documents
                    .Include(d => d.University)
                    .Include(d => d.Subject)
                    .Where(d => d.Status == DocumentStatus.Approved)
                    .AsQueryable();

                // Keyword search
                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();
                    query = query.Where(d =>
                        d.Title.ToLower().Contains(keyword) ||
                        d.Description.ToLower().Contains(keyword));
                }

                // University filter
                if (universityId.HasValue)
                {
                    query = query.Where(d => d.UniversityId == universityId.Value);
                }

                // Subject filter
                if (subjectId.HasValue)
                {
                    query = query.Where(d => d.SubjectId == subjectId.Value);
                }

                // File type filter
                if (!string.IsNullOrEmpty(fileType))
                {
                    query = query.Where(d => d.FileType == fileType);
                }

                // Pagination
                return await query
                    .OrderByDescending(d => d.UploadDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching documents");
                throw;
            }
        }

        public async Task<int> GetTotalDocumentCountAsync()
        {
            return await _context.Documents
                .Where(d => d.Status == DocumentStatus.Approved)
                .CountAsync();
        }

        public async Task<List<Document>> GetDocumentsByUserAsync(string userId)
        {
            return await _context.Documents
                .Include(d => d.University)
                .Include(d => d.Subject)
                .Where(d => d.UserId == userId && d.Status == DocumentStatus.Approved)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        // ========== STATISTICS ==========

        public async Task<Dictionary<string, int>> GetDocumentStatsAsync()
        {
            var stats = new Dictionary<string, int>();

            try
            {
                stats["TotalDocuments"] = await _context.Documents.CountAsync();
                stats["ApprovedDocuments"] = await _context.Documents
                    .CountAsync(d => d.Status == DocumentStatus.Approved);
                stats["PendingDocuments"] = await _context.Documents
                    .CountAsync(d => d.Status == DocumentStatus.Pending);
                stats["TotalViews"] = await _context.Documents.SumAsync(d => d.ViewCount);
                stats["TotalDownloads"] = await _context.Documents.SumAsync(d => d.DownloadCount);

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document stats");
                return stats;
            }
        }

        public async Task<List<Document>> GetTopRatedDocumentsAsync(int count = 10)
        {
            return await _context.Documents
                .Include(d => d.University)
                .Include(d => d.Subject)
                .Where(d => d.Status == DocumentStatus.Approved && d.RatingCount > 0)
                .OrderByDescending(d => d.AverageRating)
                .ThenByDescending(d => d.RatingCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Document>> GetMostViewedDocumentsAsync(int count = 10)
        {
            return await _context.Documents
                .Include(d => d.University)
                .Include(d => d.Subject)
                .Where(d => d.Status == DocumentStatus.Approved)
                .OrderByDescending(d => d.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Document>> GetMostDownloadedDocumentsAsync(int count = 10)
        {
            return await _context.Documents
                .Include(d => d.University)
                .Include(d => d.Subject)
                .Where(d => d.Status == DocumentStatus.Approved)
                .OrderByDescending(d => d.DownloadCount)
                .Take(count)
                .ToListAsync();
        }

        // ========== UNIVERSITY & SUBJECT RELATED ==========

        public async Task<List<Document>> GetDocumentsByUniversityAsync(int universityId)
        {
            return await _context.Documents
                .Include(d => d.Subject)
                .Where(d => d.UniversityId == universityId && d.Status == DocumentStatus.Approved)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<List<Document>> GetDocumentsBySubjectAsync(int subjectId)
        {
            return await _context.Documents
                .Include(d => d.University)
                .Where(d => d.SubjectId == subjectId && d.Status == DocumentStatus.Approved)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }
    }
}