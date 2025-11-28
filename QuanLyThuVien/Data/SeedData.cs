using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyThuVien.Models;
using System;
using System.Linq;

namespace QuanLyThuVien.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Kiểm tra đã có dữ liệu chưa
                if (context.Universities.Any())
                {
                    Console.WriteLine("Đã có dữ liệu universities, bỏ qua seed.");
                    return;
                }

                Console.WriteLine("Bắt đầu seed data...");

                // Thêm các trường đại học - SỬA: BỎ DESCRIPTION
                var universities = new University[]
                {
                    new University { Name = "Đại học Bách Khoa Hà Nội", Code = "HUST" },
                    new University { Name = "Đại học Kinh tế Quốc dân", Code = "NEU" },
                    new University { Name = "Đại học Ngoại thương", Code = "FTU" },
                    new University { Name = "Đại học Quốc gia Hà Nội", Code = "VNU" },
                    new University { Name = "Đại học FPT", Code = "FPT" },
                    new University { Name = "Đại học Công nghệ", Code = "UET" },
                    new University { Name = "Đại học Xây dựng", Code = "NUCE" },
                    new University { Name = "Đại học Giao thông Vận tải", Code = "UTC" },
                    new University { Name = "Đại học Công nghệ Tp Hồ Chí Minh", Code = "HUTECH" }
                };

                context.Universities.AddRange(universities);
                context.SaveChanges();
                Console.WriteLine($"Đã thêm {universities.Length} trường đại học.");

                // Thêm môn học cho mỗi trường
                var subjects = new Subject[]
                {
                    // HUST - Bách Khoa
                    new Subject { Name = "Lập trình Cơ bản", Code = "IT100", UniversityId = 1 },
                    new Subject { Name = "Cấu trúc Dữ liệu", Code = "IT201", UniversityId = 1 },
                    new Subject { Name = "Giải tích 1", Code = "MATH101", UniversityId = 1 },
                    new Subject { Name = "Vật lý Đại cương", Code = "PHY101", UniversityId = 1 },

                    // NEU - Kinh tế Quốc dân
                    new Subject { Name = "Kinh tế Vi mô", Code = "ECO101", UniversityId = 2 },
                    new Subject { Name = "Kinh tế Vĩ mô", Code = "ECO102", UniversityId = 2 },
                    new Subject { Name = "Quản trị Học", Code = "MGT101", UniversityId = 2 },

                    // FTU - Ngoại thương
                    new Subject { Name = "Kinh tế Quốc tế", Code = "IE101", UniversityId = 3 },
                    new Subject { Name = "Marketing Quốc tế", Code = "MKT201", UniversityId = 3 },
                    new Subject { Name = "Thanh toán Quốc tế", Code = "IP301", UniversityId = 3 },

                    // VNU - Quốc gia
                    new Subject { Name = "Triết học Mác-Lênin", Code = "PHI101", UniversityId = 4 },
                    new Subject { Name = "Tâm lý học Đại cương", Code = "PSY101", UniversityId = 4 },

                    // FPT
                    new Subject { Name = "Lập trình Java", Code = "PRJ301", UniversityId = 5 },
                    new Subject { Name = "Cơ sở Dữ liệu", Code = "DBI202", UniversityId = 5 }
                };

                context.Subjects.AddRange(subjects);
                context.SaveChanges();
                Console.WriteLine($"Đã thêm {subjects.Length} môn học.");

                Console.WriteLine("Seed data hoàn thành!");
            }
        }
    }
}