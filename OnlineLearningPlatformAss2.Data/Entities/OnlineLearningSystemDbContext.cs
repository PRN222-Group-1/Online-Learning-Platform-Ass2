using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OnlineLearningPlatformAss2.Data.Entities;

public partial class OnlineLearningSystemDbContext : DbContext
{
    public OnlineLearningSystemDbContext(DbContextOptions<OnlineLearningSystemDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AssessmentOption> AssessmentOptions { get; set; }

    public virtual DbSet<AssessmentQuestion> AssessmentQuestions { get; set; }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseReview> CourseReviews { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<LearningPath> LearningPaths { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<LessonComment> LessonComments { get; set; }

    public virtual DbSet<LessonProgress> LessonProgresses { get; set; }

    public virtual DbSet<Module> Modules { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<PathCourse> PathCourses { get; set; }

    public virtual DbSet<Profile> Profiles { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<QuizAttempt> QuizAttempts { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAnswer> UserAnswers { get; set; }

    public virtual DbSet<UserAssessment> UserAssessments { get; set; }

    public virtual DbSet<UserLearningPathEnrollment> UserLearningPathEnrollments { get; set; }

    public virtual DbSet<Wishlist> Wishlists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssessmentOption>(entity =>
        {
            entity.HasKey(e => e.OptionId);

            entity.ToTable("Assessment_Options");

            entity.HasIndex(e => e.QuestionId, "IX_Assessment_Options_question_id");

            entity.Property(e => e.OptionId)
                .ValueGeneratedNever()
                .HasColumnName("option_id");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .HasColumnName("category");
            entity.Property(e => e.OptionText)
                .HasMaxLength(300)
                .HasColumnName("option_text");
            entity.Property(e => e.OrderIndex).HasColumnName("order_index");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.SkillLevel)
                .HasMaxLength(50)
                .HasColumnName("skill_level");

            entity.HasOne(d => d.Question).WithMany(p => p.AssessmentOptions).HasForeignKey(d => d.QuestionId);
        });

        modelBuilder.Entity<AssessmentQuestion>(entity =>
        {
            entity.HasKey(e => e.QuestionId);

            entity.ToTable("Assessment_Questions");

            entity.HasIndex(e => e.CategoryId, "IX_Assessment_Questions_category_id");

            entity.Property(e => e.QuestionId)
                .ValueGeneratedNever()
                .HasColumnName("question_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.OrderIndex).HasColumnName("order_index");
            entity.Property(e => e.QuestionText)
                .HasMaxLength(500)
                .HasColumnName("question_text");
            entity.Property(e => e.QuestionType)
                .HasMaxLength(50)
                .HasColumnName("question_type");

            entity.HasOne(d => d.Category).WithMany(p => p.AssessmentQuestions)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasIndex(e => e.AuthorId, "IX_Blogs_author_id");

            entity.Property(e => e.BlogId)
                .ValueGeneratedNever()
                .HasColumnName("blog_id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.Author).WithMany(p => p.Blogs).HasForeignKey(d => d.AuthorId);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.ParentId, "IX_Categories_parent_id");

            entity.Property(e => e.CategoryId)
                .ValueGeneratedNever()
                .HasColumnName("category_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent).HasForeignKey(d => d.ParentId);
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasIndex(e => e.EnrollmentId, "IX_Certificates_enrollment_id");

            entity.Property(e => e.CertificateId)
                .ValueGeneratedNever()
                .HasColumnName("certificate_id");
            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.IssueDate).HasColumnName("issue_date");
            entity.Property(e => e.PdfUrl).HasColumnName("pdf_url");
            entity.Property(e => e.SerialNumber)
                .HasMaxLength(100)
                .HasColumnName("serial_number");

            entity.HasOne(d => d.Enrollment).WithMany(p => p.Certificates).HasForeignKey(d => d.EnrollmentId);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Chat_Mes__0BBF6EE6D1CE85A6");

            entity.ToTable("Chat_Messages");

            entity.HasIndex(e => e.ReceiverId, "IX_Chat_Messages_receiver_id");

            entity.HasIndex(e => e.SenderId, "IX_Chat_Messages_sender_id");

            entity.Property(e => e.MessageId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("message_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.IsFromAdmin).HasColumnName("is_from_admin");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("sent_at");

            entity.HasOne(d => d.Receiver).WithMany(p => p.ChatMessageReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .HasConstraintName("FK_ChatMessages_Receiver");

            entity.HasOne(d => d.Sender).WithMany(p => p.ChatMessageSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatMessages_Sender");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasIndex(e => e.CategoryId, "IX_Courses_category_id");

            entity.HasIndex(e => e.InstructorId, "IX_Courses_instructor_id");

            entity.Property(e => e.CourseId)
                .ValueGeneratedNever()
                .HasColumnName("course_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.InstructorId).HasColumnName("instructor_id");
            entity.Property(e => e.IsFeatured).HasColumnName("is_featured");
            entity.Property(e => e.Language)
                .HasMaxLength(50)
                .HasDefaultValue("English")
                .HasColumnName("language");
            entity.Property(e => e.Level)
                .HasMaxLength(50)
                .HasDefaultValue("All Levels")
                .HasColumnName("level");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.Category).WithMany(p => p.Courses).HasForeignKey(d => d.CategoryId);

            entity.HasOne(d => d.Instructor).WithMany(p => p.Courses)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<CourseReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Course_R__60883D90AA711BB2");

            entity.ToTable("Course_Reviews");

            entity.HasIndex(e => e.CourseId, "IX_Course_Reviews_course_id");

            entity.HasIndex(e => e.UserId, "IX_Course_Reviews_user_id");

            entity.Property(e => e.ReviewId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("review_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseReviews)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK_CourseReviews_Courses");

            entity.HasOne(d => d.User).WithMany(p => p.CourseReviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CourseReviews_Users");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasIndex(e => e.CourseId, "IX_Enrollments_course_id");

            entity.HasIndex(e => e.UserId, "IX_Enrollments_user_id");

            entity.Property(e => e.EnrollmentId)
                .ValueGeneratedNever()
                .HasColumnName("enrollment_id");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.EnrolledAt).HasColumnName("enrolled_at");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Course).WithMany(p => p.Enrollments).HasForeignKey(d => d.CourseId);

            entity.HasOne(d => d.User).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<LearningPath>(entity =>
        {
            entity.HasKey(e => e.PathId);

            entity.ToTable("Learning_Paths");

            entity.HasIndex(e => e.CreatedByUserId, "IX_Learning_Paths_created_by_user_id");

            entity.Property(e => e.PathId)
                .ValueGeneratedNever()
                .HasColumnName("path_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsCustomPath).HasColumnName("is_custom_path");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.SourceAssessmentId).HasColumnName("source_assessment_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.LearningPaths).HasForeignKey(d => d.CreatedByUserId);
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasIndex(e => e.ModuleId, "IX_Lessons_module_id");

            entity.Property(e => e.LessonId)
                .ValueGeneratedNever()
                .HasColumnName("lesson_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.ContentUrl).HasColumnName("content_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.ModuleId).HasColumnName("module_id");
            entity.Property(e => e.OrderIndex).HasColumnName("order_index");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.VideoUrl).HasColumnName("video_url");

            entity.HasOne(d => d.Module).WithMany(p => p.Lessons).HasForeignKey(d => d.ModuleId);
        });

        modelBuilder.Entity<LessonComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Lesson_C__E79576870300F7CD");

            entity.ToTable("Lesson_Comments");

            entity.HasIndex(e => e.LessonId, "IX_Lesson_Comments_lesson_id");

            entity.HasIndex(e => e.ParentId, "IX_Lesson_Comments_parent_id");

            entity.HasIndex(e => e.UserId, "IX_Lesson_Comments_user_id");

            entity.Property(e => e.CommentId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("comment_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.LessonId).HasColumnName("lesson_id");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Lesson).WithMany(p => p.LessonComments)
                .HasForeignKey(d => d.LessonId)
                .HasConstraintName("FK_LessonComments_Lessons");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_LessonComments_Parent");

            entity.HasOne(d => d.User).WithMany(p => p.LessonComments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LessonComments_Users");
        });

        modelBuilder.Entity<LessonProgress>(entity =>
        {
            entity.HasKey(e => e.ProgressId);

            entity.ToTable("Lesson_Progress");

            entity.HasIndex(e => e.EnrollmentId, "IX_Lesson_Progress_enrollment_id");

            entity.HasIndex(e => e.LessonId, "IX_Lesson_Progress_lesson_id");

            entity.Property(e => e.ProgressId)
                .ValueGeneratedNever()
                .HasColumnName("progress_id");
            entity.Property(e => e.AiSummary).HasColumnName("ai_summary");
            entity.Property(e => e.AiSummaryStatus)
                .HasMaxLength(50)
                .HasColumnName("ai_summary_status");
            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.IsCompleted).HasColumnName("is_completed");
            entity.Property(e => e.LastAccessedAt).HasColumnName("last_accessed_at");
            entity.Property(e => e.LastWatchedPosition).HasColumnName("last_watched_position");
            entity.Property(e => e.LessonId).HasColumnName("lesson_id");
            entity.Property(e => e.Transcript).HasColumnName("transcript");

            entity.HasOne(d => d.Enrollment).WithMany(p => p.LessonProgresses).HasForeignKey(d => d.EnrollmentId);

            entity.HasOne(d => d.Lesson).WithMany(p => p.LessonProgresses)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasIndex(e => e.CourseId, "IX_Modules_course_id");

            entity.Property(e => e.ModuleId)
                .ValueGeneratedNever()
                .HasColumnName("module_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.OrderIndex).HasColumnName("order_index");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.Course).WithMany(p => p.Modules).HasForeignKey(d => d.CourseId);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842FECA777A8");

            entity.HasIndex(e => e.UserId, "IX_Notifications_user_id");

            entity.Property(e => e.NotificationId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("notification_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasDefaultValue("General")
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasIndex(e => e.QuestionId, "IX_Options_question_id");

            entity.Property(e => e.OptionId)
                .ValueGeneratedNever()
                .HasColumnName("option_id");
            entity.Property(e => e.IsCorrect).HasColumnName("is_correct");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.Text).HasColumnName("text");

            entity.HasOne(d => d.Question).WithMany(p => p.Options).HasForeignKey(d => d.QuestionId);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.CourseId, "IX_Orders_course_id");

            entity.HasIndex(e => e.PathId, "IX_Orders_path_id");

            entity.HasIndex(e => e.UserId, "IX_Orders_user_id");

            entity.Property(e => e.OrderId)
                .ValueGeneratedNever()
                .HasColumnName("order_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.PathId).HasColumnName("path_id");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_amount");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Course).WithMany(p => p.Orders).HasForeignKey(d => d.CourseId);

            entity.HasOne(d => d.Path).WithMany(p => p.Orders).HasForeignKey(d => d.PathId);

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<PathCourse>(entity =>
        {
            entity.HasKey(e => new { e.PathId, e.CourseId });

            entity.ToTable("Path_Courses");

            entity.HasIndex(e => e.CourseId, "IX_Path_Courses_course_id");

            entity.Property(e => e.PathId).HasColumnName("path_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.OrderIndex).HasColumnName("order_index");

            entity.HasOne(d => d.Course).WithMany(p => p.PathCourses).HasForeignKey(d => d.CourseId);

            entity.HasOne(d => d.Path).WithMany(p => p.PathCourses).HasForeignKey(d => d.PathId);
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.ToTable("profiles");

            entity.HasIndex(e => e.UserId, "IX_profiles_UserId").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);

            entity.HasOne(d => d.User).WithOne(p => p.Profile).HasForeignKey<Profile>(d => d.UserId);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasIndex(e => e.QuizId, "IX_Questions_quiz_id");

            entity.Property(e => e.QuestionId)
                .ValueGeneratedNever()
                .HasColumnName("question_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.QuizId).HasColumnName("quiz_id");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasColumnName("type");

            entity.HasOne(d => d.Quiz).WithMany(p => p.Questions).HasForeignKey(d => d.QuizId);
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasIndex(e => e.LessonId, "IX_Quizzes_lesson_id");

            entity.Property(e => e.QuizId)
                .ValueGeneratedNever()
                .HasColumnName("quiz_id");
            entity.Property(e => e.LessonId).HasColumnName("lesson_id");
            entity.Property(e => e.PassingScore).HasColumnName("passing_score");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.Lesson).WithMany(p => p.Quizzes).HasForeignKey(d => d.LessonId);
        });

        modelBuilder.Entity<QuizAttempt>(entity =>
        {
            entity.HasKey(e => e.AttemptId);

            entity.ToTable("Quiz_Attempts");

            entity.HasIndex(e => e.QuizId, "IX_Quiz_Attempts_quiz_id");

            entity.HasIndex(e => e.UserId, "IX_Quiz_Attempts_user_id");

            entity.Property(e => e.AttemptId)
                .ValueGeneratedNever()
                .HasColumnName("attempt_id");
            entity.Property(e => e.AttemptedAt).HasColumnName("attempted_at");
            entity.Property(e => e.Passed).HasColumnName("passed");
            entity.Property(e => e.QuizId).HasColumnName("quiz_id");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizAttempts).HasForeignKey(d => d.QuizId);

            entity.HasOne(d => d.User).WithMany(p => p.QuizAttempts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasIndex(e => e.OrderId, "IX_Transactions_order_id");

            entity.HasIndex(e => e.TransactionGateId, "IX_Transactions_transaction_gate_id")
                .IsUnique()
                .HasFilter("([transaction_gate_id] IS NOT NULL)");

            entity.Property(e => e.TransactionId)
                .ValueGeneratedNever()
                .HasColumnName("transaction_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.TransactionGateId)
                .HasMaxLength(100)
                .HasColumnName("transaction_gate_id");

            entity.HasOne(d => d.Order).WithMany(p => p.Transactions).HasForeignKey(d => d.OrderId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "IX_users_Email").IsUnique();

            entity.HasIndex(e => e.RoleId, "IX_users_RoleId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AssessmentCompletedAt).HasColumnName("assessment_completed_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HasCompletedAssessment).HasColumnName("has_completed_assessment");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.HasKey(e => e.AnswerId);

            entity.ToTable("User_Answers");

            entity.HasIndex(e => e.AssessmentId, "IX_User_Answers_assessment_id");

            entity.HasIndex(e => e.QuestionId, "IX_User_Answers_question_id");

            entity.HasIndex(e => e.SelectedOptionId, "IX_User_Answers_selected_option_id");

            entity.Property(e => e.AnswerId)
                .ValueGeneratedNever()
                .HasColumnName("answer_id");
            entity.Property(e => e.AssessmentId).HasColumnName("assessment_id");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.SelectedOptionId).HasColumnName("selected_option_id");

            entity.HasOne(d => d.Assessment).WithMany(p => p.UserAnswers).HasForeignKey(d => d.AssessmentId);

            entity.HasOne(d => d.Question).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.SelectedOption).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.SelectedOptionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<UserAssessment>(entity =>
        {
            entity.HasKey(e => e.AssessmentId);

            entity.ToTable("User_Assessments");

            entity.HasIndex(e => e.UserId, "IX_User_Assessments_user_id");

            entity.Property(e => e.AssessmentId)
                .ValueGeneratedNever()
                .HasColumnName("assessment_id");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.ResultSummary).HasColumnName("result_summary");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserAssessments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<UserLearningPathEnrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId);

            entity.ToTable("User_Learning_Path_Enrollments");

            entity.HasIndex(e => e.PathId, "IX_User_Learning_Path_Enrollments_path_id");

            entity.HasIndex(e => e.UserId, "IX_User_Learning_Path_Enrollments_user_id");

            entity.Property(e => e.EnrollmentId)
                .ValueGeneratedNever()
                .HasColumnName("enrollment_id");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.EnrolledAt).HasColumnName("enrolled_at");
            entity.Property(e => e.PathId).HasColumnName("path_id");
            entity.Property(e => e.ProgressPercentage).HasColumnName("progress_percentage");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Path).WithMany(p => p.UserLearningPathEnrollments).HasForeignKey(d => d.PathId);

            entity.HasOne(d => d.User).WithMany(p => p.UserLearningPathEnrollments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasKey(e => e.WishlistId).HasName("PK__Wishlist__6151514ECFDDC1A5");

            entity.HasIndex(e => e.CourseId, "IX_Wishlists_course_id");

            entity.HasIndex(e => e.UserId, "IX_Wishlists_user_id");

            entity.Property(e => e.WishlistId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("wishlist_id");
            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("added_at");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Course).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK_Wishlists_Courses");

            entity.HasOne(d => d.User).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Wishlists_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
