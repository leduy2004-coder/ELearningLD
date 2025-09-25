using Domain.Entities;
using Infrastructure.SeedConfiguration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<User, Role, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Các bảng domain
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamQuestion> ExamQuestions { get; set; }
        public DbSet<ExamAttempt> ExamAttempts { get; set; }
        public DbSet<AttemptAnswer> AttemptAnswers { get; set; }
        public DbSet<LessonVideo> LessonVideos { get; set; } // Thêm mới
        public DbSet<Comment> Comments { get; set; } // Thêm mới
        public DbSet<Rating> Ratings { get; set; } // Thêm mới
        public DbSet<UserFollow> UserFollows { get; set; } // Thêm mới

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new RoleConfiguration());

            // --- Composite keys ---
            modelBuilder.Entity<ExamQuestion>()
                .HasKey(eq => new { eq.ExamId, eq.QuestionId });

            modelBuilder.Entity<AttemptAnswer>()
                .HasKey(aa => new { aa.AttemptId, aa.QuestionId });

            modelBuilder.Entity<UserFollow>()
                .HasKey(uf => new { uf.FollowerId, uf.FollowedId }); // Khóa chính composite cho UserFollow

            // --- Cascade paths cho ExamQuestion ---
            modelBuilder.Entity<ExamQuestion>()
                .HasOne(eq => eq.Exam)
                .WithMany(e => e.ExamQuestions)
                .HasForeignKey(eq => eq.ExamId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Exam → Xóa ExamQuestions

            modelBuilder.Entity<ExamQuestion>()
                .HasOne(eq => eq.Question)
                .WithMany()
                .HasForeignKey(eq => eq.QuestionId)
                .OnDelete(DeleteBehavior.Restrict); // Xóa Question không tự động xóa

            // --- Cascade paths cho AttemptAnswer ---
            modelBuilder.Entity<AttemptAnswer>()
                .HasOne(aa => aa.Attempt)
                .WithMany(a => a.AttemptAnswers)
                .HasForeignKey(aa => aa.AttemptId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Attempt → Xóa AttemptAnswer

            modelBuilder.Entity<AttemptAnswer>()
                .HasOne(aa => aa.Question)
                .WithMany()
                .HasForeignKey(aa => aa.QuestionId)
                .OnDelete(DeleteBehavior.Restrict); // Xóa Question không tự động xóa

            modelBuilder.Entity<AttemptAnswer>()
                .HasOne(aa => aa.Answer)
                .WithMany()
                .HasForeignKey(aa => aa.AnswerId)
                .OnDelete(DeleteBehavior.Restrict); // Xóa Answer không tự động xóa

            // --- Question → Answer ---
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Question → Xóa Answer

            // --- ExamAttempt → Exam ---
            modelBuilder.Entity<ExamAttempt>()
                .HasOne(ea => ea.Exam)
                .WithMany(e => e.ExamAttempts)
                .HasForeignKey(ea => ea.ExamId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- ExamAttempt → User ---
            modelBuilder.Entity<ExamAttempt>()
                .HasOne(ea => ea.Student)
                .WithMany(u => u.ExamAttempts)
                .HasForeignKey(ea => ea.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Question → User ---
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Creator)
                .WithMany(u => u.QuestionsCreated)
                .HasForeignKey(q => q.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Exam → User ---
            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Creator)
                .WithMany(u => u.ExamsCreated)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // --- LessonVideo → User ---
            modelBuilder.Entity<LessonVideo>()
                .HasOne(lv => lv.Creator)
                .WithMany(u => u.LessonVideosCreated)
                .HasForeignKey(lv => lv.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // --- UserFollow → User (Follower và Followed) ---
            modelBuilder.Entity<UserFollow>()
                .HasOne(uf => uf.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(uf => uf.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserFollow>()
                .HasOne(uf => uf.Followed)
                .WithMany(u => u.Followers)
                .HasForeignKey(uf => uf.FollowedId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Comment → User ---
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Rating → User ---
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Index cho InvitationCode (tối ưu tìm kiếm) ---
            modelBuilder.Entity<Exam>()
                .HasIndex(e => e.InvitationCode)
                .IsUnique(); // Đảm bảo InvitationCode là duy nhất (nếu có)

            // --- Index cho TargetId và TargetType (Comment, Rating) ---
            modelBuilder.Entity<Comment>()
                .HasIndex(c => new { c.TargetId, c.TargetType });

            modelBuilder.Entity<Rating>()
                .HasIndex(r => new { r.TargetId, r.TargetType });
        }
    }
}