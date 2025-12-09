using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;

namespace QuanLyThuVien.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<University> Universities { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Rating> Ratings { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Rating ↔ Document relationship
            builder.Entity<Rating>()
               .HasOne(r => r.Document)
               .WithMany(d => d.Ratings)
               .HasForeignKey(r => r.DocumentId)
               .OnDelete(DeleteBehavior.Cascade); // ✅ Khi xóa Document, xóa luôn Ratings

            // Rating ↔ User relationship  
            builder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade); // ✅ Khi xóa User, xóa luôn Ratings
        }
    }
}