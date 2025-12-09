using QuanLyThuVien.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLyThuVien.Services
{
    public interface IDocumentService
    {
        // CRUD Operations
        Task<Document> GetDocumentByIdAsync(int id);
        Task<List<Document>> GetAllDocumentsAsync();
        Task<List<Document>> GetRecentDocumentsAsync(int count = 10);
        Task<Document> CreateDocumentAsync(Document document, IFormFile file, string userId);
        Task UpdateDocumentAsync(int id, Document document, IFormFile? file = null);
        Task DeleteDocumentAsync(int id);

        // File Operations
        Task<string> UploadFileAsync(IFormFile file);
        Task DeleteFileAsync(string filePath);

        // Statistics & Tracking
        Task IncrementViewCountAsync(int documentId);
        Task IncrementDownloadCountAsync(int documentId);
        Task UpdateDocumentStatusAsync(int documentId, DocumentStatus status);

        // Search & Filter
        Task<List<Document>> SearchDocumentsAsync(string keyword, int? universityId, int? subjectId,
                                                  string fileType = null, int page = 1, int pageSize = 20);
        Task<int> GetTotalDocumentCountAsync();
        Task<List<Document>> GetDocumentsByUserAsync(string userId);

        // Statistics
        Task<Dictionary<string, int>> GetDocumentStatsAsync();
        Task<List<Document>> GetTopRatedDocumentsAsync(int count = 10);
        Task<List<Document>> GetMostViewedDocumentsAsync(int count = 10);
        Task<List<Document>> GetMostDownloadedDocumentsAsync(int count = 10);

        // University & Subject related
        Task<List<Document>> GetDocumentsByUniversityAsync(int universityId);
        Task<List<Document>> GetDocumentsBySubjectAsync(int subjectId);
    }
}