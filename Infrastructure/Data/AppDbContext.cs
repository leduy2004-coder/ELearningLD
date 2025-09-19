using Domain.Entities;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);



            // --- Composite keys ---
            modelBuilder.Entity<ExamQuestion>()
                .HasKey(eq => new { eq.ExamId, eq.QuestionId });

            modelBuilder.Entity<AttemptAnswer>()
                .HasKey(aa => new { aa.AttemptId, aa.QuestionId });

            // --- Cascade paths cho ExamQuestion ---
            modelBuilder.Entity<ExamQuestion>()
                .HasOne(eq => eq.Exam)
                .WithMany(e => e.ExamQuestions)
                .HasForeignKey(eq => eq.ExamId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Exam → ExamQuestions

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
                .OnDelete(DeleteBehavior.Cascade); // Xóa Attempt → AttemptAnswer

            modelBuilder.Entity<AttemptAnswer>()
                .HasOne(aa => aa.Question)
                .WithMany()
                .HasForeignKey(aa => aa.QuestionId)
                .OnDelete(DeleteBehavior.Restrict); // Xóa Question không tự động xóa

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
        }
    }
}
