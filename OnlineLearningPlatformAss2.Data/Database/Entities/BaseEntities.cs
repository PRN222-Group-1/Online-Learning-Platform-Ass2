namespace OnlineLearningPlatformAss2.Data.Database.Entities;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class Profile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? AvatarUrl { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
}

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}

public class Course
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public Guid CategoryId { get; set; }
    public Guid InstructorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsFeatured { get; set; }
    public string Status { get; set; } = "Published"; // Pending, Published, Rejected
    public string Level { get; set; } = "All Levels";
    public string Language { get; set; } = "English";
    public string? RejectionReason { get; set; }
    
    // Navigation properties
    public Category Category { get; set; } = null!;
    public User Instructor { get; set; } = null!;
    public ICollection<Module> Modules { get; set; } = new List<Module>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<PathCourse> PathCourses { get; set; } = new List<PathCourse>();
    public ICollection<CourseReview> Reviews { get; set; } = new List<CourseReview>();
}

public class Module
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int OrderIndex { get; set; }
    public Guid CourseId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Course Course { get; set; } = null!;
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}

public class Lesson
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int OrderIndex { get; set; }
    public string? VideoUrl { get; set; }
    public Guid ModuleId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Module Module { get; set; } = null!;
    public ICollection<LessonProgress> Progresses { get; set; } = new List<LessonProgress>();
}

public class Enrollment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "Active"; // Active, Completed, Dropped
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
}

public class LessonProgress
{
    public Guid Id { get; set; }
    public Guid EnrollmentId { get; set; }
    public Guid LessonId { get; set; }
    public bool IsCompleted { get; set; }
    public int WatchedSeconds { get; set; }
    public DateTime LastAccessedAt { get; set; }
    
    // Navigation properties
    public Enrollment Enrollment { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
}

public class LearningPath
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Published, Archived
    public bool IsCustomPath { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public ICollection<PathCourse> PathCourses { get; set; } = new List<PathCourse>();
    public ICollection<UserLearningPathEnrollment> UserEnrollments { get; set; } = new List<UserLearningPathEnrollment>();
}

public class PathCourse
{
    public Guid PathId { get; set; }
    public Guid CourseId { get; set; }
    public int OrderIndex { get; set; }
    
    // Navigation properties
    public LearningPath LearningPath { get; set; } = null!;
    public Course Course { get; set; } = null!;
}

public class UserLearningPathEnrollment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PathId { get; set; }
    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "Active";
    
    // Navigation properties
    public User User { get; set; } = null!;
    public LearningPath LearningPath { get; set; } = null!;
}

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? CourseId { get; set; }
    public Guid? LearningPathId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Course? Course { get; set; }
    public LearningPath? LearningPath { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

public class Transaction
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public string? PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Order Order { get; set; } = null!;
}

// Additional entities for assessment system
public class AssessmentQuestion
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = null!;
    public string QuestionType { get; set; } = null!;
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<AssessmentOption> Options { get; set; } = new List<AssessmentOption>();
}

public class AssessmentOption
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string OptionText { get; set; } = null!;
    public string SkillLevel { get; set; } = null!;
    public string Category { get; set; } = null!;
    
    public AssessmentQuestion Question { get; set; } = null!;
}

public class UserAssessment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CompletedAt { get; set; }
    public string ResultSummary { get; set; } = null!;
    
    public User User { get; set; } = null!;
    public ICollection<UserAnswer> Answers { get; set; } = new List<UserAnswer>();
}

public class UserAnswer
{
    public Guid Id { get; set; }
    public Guid AssessmentId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid OptionId { get; set; }
    
    public UserAssessment Assessment { get; set; } = null!;
    public AssessmentQuestion Question { get; set; } = null!;
    public AssessmentOption Option { get; set; } = null!;
}

// Additional entities
public class Quiz
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}

public class Question
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
    public Guid QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    public ICollection<Option> Options { get; set; } = new List<Option>();
}

public class Option
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
}

public class QuizAttempt
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid QuizId { get; set; }
    public int Score { get; set; }
    public DateTime AttemptedAt { get; set; }
    
    public User User { get; set; } = null!;
    public Quiz Quiz { get; set; } = null!;
}

public class Certificate
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public DateTime IssuedAt { get; set; }
    public string CertificateUrl { get; set; } = null!;
    
    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
}

public class Blog
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public User Author { get; set; } = null!;
}

public class Wishlist
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
}

public class LessonComment
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public Guid? ParentId { get; set; }
    
    // Navigation properties
    public Lesson Lesson { get; set; } = null!;
    public User User { get; set; } = null!;
    public LessonComment? Parent { get; set; }
    public ICollection<LessonComment> Replies { get; set; } = new List<LessonComment>();
}

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; } = null!;
    public string Type { get; set; } = "General"; // Enrollment, Approval, System
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}

public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid? ReceiverId { get; set; }
    public string Content { get; set; } = null!;
    public bool IsFromAdmin { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public User Sender { get; set; } = null!;
    public User? Receiver { get; set; }
}
