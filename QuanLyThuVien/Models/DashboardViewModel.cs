using System.Collections.Generic;
using QuanLyThuVien.Models;

namespace QuanLyThuVien.Models
{
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