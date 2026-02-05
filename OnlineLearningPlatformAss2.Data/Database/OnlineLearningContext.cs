using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatformAss2.Data.Database.Entities;

namespace OnlineLearningPlatformAss2.Data.Database;

public class OnlineLearningContext(DbContextOptions<OnlineLearningContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Module> Modules { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Option> Options { get; set; }
    public DbSet<QuizAttempt> QuizAttempts { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<LessonProgress> LessonProgresses { get; set; }
    public DbSet<Certificate> Certificates { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<LearningPath> LearningPaths { get; set; }
    public DbSet<PathCourse> PathCourses { get; set; }
    public DbSet<UserLearningPathEnrollment> UserLearningPathEnrollments { get; set; }
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<AssessmentQuestion> AssessmentQuestions { get; set; }
    public DbSet<AssessmentOption> AssessmentOptions { get; set; }
    public DbSet<UserAssessment> UserAssessments { get; set; }
    public DbSet<UserAnswer> UserAnswers { get; set; }
    public DbSet<CourseReview> CourseReviews { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<LessonComment> LessonComments { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Wishlist>()
            .HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Wishlist>()
            .HasOne(w => w.Course)
            .WithMany()
            .HasForeignKey(w => w.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<CourseReview>()
            .HasOne(cr => cr.User)
            .WithMany()
            .HasForeignKey(cr => cr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CourseReview>()
            .HasOne(cr => cr.Course)
            .WithMany(c => c.Reviews)
            .HasForeignKey(cr => cr.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Apply configurations from separate classes if any
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OnlineLearningContext).Assembly);

        // Configure Many-to-Many for PathCourses (LearningPath <-> Course)
        modelBuilder.Entity<PathCourse>()
            .HasKey(pc => new { pc.PathId, pc.CourseId });

        modelBuilder.Entity<PathCourse>()
            .HasOne(pc => pc.LearningPath)
            .WithMany(lp => lp.PathCourses)
            .HasForeignKey(pc => pc.PathId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PathCourse>()
            .HasOne(pc => pc.Course)
            .WithMany(c => c.PathCourses)
            .HasForeignKey(pc => pc.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure UserLearningPathEnrollment
        modelBuilder.Entity<UserLearningPathEnrollment>()
            .HasOne(ulpe => ulpe.User)
            .WithMany(u => u.LearningPathEnrollments)
            .HasForeignKey(ulpe => ulpe.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserLearningPathEnrollment>()
            .HasOne(ulpe => ulpe.LearningPath)
            .WithMany(lp => lp.UserEnrollments)
            .HasForeignKey(ulpe => ulpe.PathId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ensure decimal precision for prices/amounts
        modelBuilder.Entity<Course>()
            .Property(c => c.Price)
            .HasColumnType("decimal(18, 2)");
            
        modelBuilder.Entity<LearningPath>()
            .Property(lp => lp.Price)
            .HasColumnType("decimal(18, 2)");

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasColumnType("decimal(18, 2)");

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasColumnType("decimal(18, 2)");

        // Cycle Breaking Configurations
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict); 

        modelBuilder.Entity<QuizAttempt>()
            .HasOne(qa => qa.User)
            .WithMany()
            .HasForeignKey(qa => qa.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany() 
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Course>()
            .HasOne(c => c.Instructor)
            .WithMany() 
            .HasForeignKey(c => c.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LessonProgress>()
            .HasOne(lp => lp.Lesson)
            .WithMany(l => l.Progresses)
            .HasForeignKey(lp => lp.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
