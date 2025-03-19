using Microsoft.EntityFrameworkCore;
using EduMastersAPI.Models;

namespace EduMastersAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<University> Universities { get; set; }
        public DbSet<UniversityBranch> UniversityBranches { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentFile> StudentFiles { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<ApplicationFile> ApplicationFiles { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<Announcement> Announcements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // علاقات بين الجداول
            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.University)
                .WithMany()
                .HasForeignKey(a => a.university_id);

            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.CreatedByUser)
                .WithMany()
                .HasForeignKey(a => a.created_by);

            modelBuilder.Entity<Student>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(s => s.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<University>()
                .HasMany(u => u.Branches)
                .WithOne(b => b.University)
                .HasForeignKey(b => b.UniversityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationFile>()
    .HasOne(a => a.CreatedByUser)  // تحديد العلاقة بين ApplicationFile و User
    .WithMany()                    // حيث يمكن أن يكون هناك مستخدم واحد وملفات متعددة
    .HasForeignKey(a => a.CreatedByUserId); // تعيين المفتاح الأجنبي
        }
    }
}
