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
        public DbSet<Bookmark> Bookmarks { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cấu hình quan hệ
            builder.Entity<Bookmark>()
                .HasIndex(b => new { b.UserId, b.DocumentId })
                .IsUnique(); // Mỗi user chỉ bookmark 1 document 1 lần

            // Cấu hình foreign key để tránh multiple cascade paths
            builder.Entity<Bookmark>()
                .HasOne(b => b.User)
                .WithMany()  // XÓA: .WithMany(u => u.Bookmarks)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Bookmark>()
                .HasOne(b => b.Document)
                .WithMany(d => d.Bookmarks)
                .HasForeignKey(b => b.DocumentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Cấu hình tương tự cho Ratings
            builder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany()  // XÓA: .WithMany(u => u.Ratings)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Rating>()
                .HasOne(r => r.Document)
                .WithMany(d => d.Ratings)
                .HasForeignKey(r => r.DocumentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Cấu hình cho Documents
            builder.Entity<Document>()
                .HasOne(d => d.User)
                .WithMany()  // XÓA: .WithMany(u => u.Documents)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}