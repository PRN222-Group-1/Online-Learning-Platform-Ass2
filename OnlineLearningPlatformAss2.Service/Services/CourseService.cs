using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.DTOs.Category;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace OnlineLearningPlatformAss2.Service.Services;

public class CourseService : ICourseService
{
    private readonly OnlineLearningContext _context;
    private readonly IReviewService _reviewService;

    public CourseService(OnlineLearningContext context, IReviewService reviewService)
    {
        _context = context;
        _reviewService = reviewService;
    }

    public async Task<IEnumerable<CourseViewModel>> GetFeaturedCoursesAsync(int? limit = null)
    {
        var query = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Where(c => c.IsFeatured && c.Status == "Published");

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }
        else
        {
            query = query.Take(6);
        }

        var courses = await query
            .Select(c => new CourseViewModel
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Price = c.Price,
                ImageUrl = c.ImageUrl,
                CategoryName = c.Category.Name,
                InstructorName = c.Instructor.Username,
                Rating = c.Reviews.Any() ? (decimal)c.Reviews.Average(r => r.Rating) : 0m,
                StudentCount = _context.Enrollments.Count(e => e.CourseId == c.Id),
                IsFeatured = true
            })
            .ToListAsync();

        return courses;
    }

    public async Task<IEnumerable<CourseViewModel>> GetAllCoursesAsync(string? searchTerm = null, Guid? categoryId = null, int? limit = null)
    {
        var query = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Where(c => c.Status == "Published")
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(c => 
                c.Title.ToLower().Contains(term) ||
                c.Description.ToLower().Contains(term) ||
                c.Category.Name.ToLower().Contains(term) ||
                c.Instructor.Username.ToLower().Contains(term)
            );
        }
        
        if (categoryId.HasValue)
        {
            query = query.Where(c => c.CategoryId == categoryId.Value);
        }

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        var courses = await query
            .Select(c => new CourseViewModel
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Price = c.Price,
                ImageUrl = c.ImageUrl,
                CategoryName = c.Category.Name,
                InstructorName = c.Instructor.Username,
                Rating = c.Reviews.Any() ? (decimal)c.Reviews.Average(r => r.Rating) : 0m,
                StudentCount = _context.Enrollments.Count(e => e.CourseId == c.Id),
                IsFeatured = false
            })
            .ToListAsync();

        return courses;
    }

    public async Task<CourseDetailViewModel?> GetCourseDetailsAsync(Guid id, Guid? userId = null)
    {
        var course = await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Include(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .Include(c => c.Reviews)
            .ThenInclude(r => r.User)
            .Where(c => c.Status == "Published")
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
            return null;

        var isEnrolled = false;
        var isInWishlist = false;
        if (userId.HasValue)
        {
            isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId.Value && e.CourseId == id);
                
            isInWishlist = await _context.Wishlists
                .AnyAsync(w => w.UserId == userId.Value && w.CourseId == id);
        }

        var studentCount = await _context.Enrollments.CountAsync(e => e.CourseId == id);
        var ratingSummary = await _reviewService.GetRatingSummaryAsync(id);

        return new CourseDetailViewModel
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Price = course.Price,
            ImageUrl = course.ImageUrl,
            CategoryName = course.Category.Name,
            InstructorName = course.Instructor.Username,
            Rating = (decimal)ratingSummary.AverageRating,
            ReviewCount = ratingSummary.TotalReviews,
            StudentCount = studentCount,
            Level = course.Level,
            Language = course.Language,
            IsEnrolled = isEnrolled,
            IsInWishlist = isInWishlist,
            WhatYouWillLearn = new List<string> { "Foundational concepts", "Real-world projects", "Best practices" },
            Requirements = new List<string> { "Basic knowledge of the field" },
            HasCertificate = true,
            Modules = course.Modules.Select(m => new ModuleViewModel
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                OrderIndex = m.OrderIndex,
                Lessons = m.Lessons.Select(l => new LessonViewModel
                {
                    Id = l.Id,
                    Title = l.Title,
                    Duration = 15,
                    OrderIndex = l.OrderIndex,
                    IsPreview = l.OrderIndex <= 2
                }).OrderBy(l => l.OrderIndex).ToList()
            }).OrderBy(m => m.OrderIndex).ToList(),
            Reviews = (await _reviewService.GetCourseReviewsAsync(id)).Select(r => new ReviewViewModel 
            {
                Id = r.Id,
                Username = r.Username,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList()
        };
    }

    public async Task<bool> ToggleWishlistAsync(Guid userId, Guid courseId)
    {
        var existing = await _context.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.CourseId == courseId);

        if (existing != null)
        {
            _context.Wishlists.Remove(existing);
        }
        else
        {
            _context.Wishlists.Add(new Wishlist
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CourseId = courseId,
                AddedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return existing == null; // returns true if added, false if removed
    }

    public async Task<IEnumerable<CourseViewModel>> GetWishlistAsync(Guid userId)
    {
        return await _context.Wishlists
            .Include(w => w.Course)
            .ThenInclude(c => c.Category)
            .Include(w => w.Course)
            .ThenInclude(c => c.Instructor)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.AddedAt)
            .Select(w => new CourseViewModel
            {
                Id = w.Course.Id,
                Title = w.Course.Title,
                Description = w.Course.Description,
                Price = w.Course.Price,
                ImageUrl = w.Course.ImageUrl,
                CategoryName = w.Course.Category.Name,
                InstructorName = w.Course.Instructor.Username,
                Rating = 4.5m, // TODO: calculate real rating if needed in list view
                StudentCount = 0, // Placeholder
                IsInWishlist = true
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<CourseViewModel>> GetInstructorCoursesAsync(Guid instructorId)
    {
        return await _context.Courses
            .Include(c => c.Category)
            .Where(c => c.InstructorId == instructorId)
            .Select(c => new CourseViewModel
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Price = c.Price,
                ImageUrl = c.ImageUrl,
                CategoryName = c.Category.Name,
                InstructorName = c.Instructor.Username,
                Rating = c.Reviews.Any() ? (decimal)c.Reviews.Average(r => r.Rating) : 0,
                StudentCount = _context.Enrollments.Count(e => e.CourseId == c.Id),
                IsFeatured = c.IsFeatured,
                Status = c.Status,
                RejectionReason = c.RejectionReason
            })
            .ToListAsync();
    }

    public async Task<bool> UpdateCourseAsync(Guid courseId, CourseUpdateDto dto, Guid instructorId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null || course.InstructorId != instructorId) return false;

        course.Title = dto.Title;
        course.Description = dto.Description;
        course.Price = dto.Price;
        course.ImageUrl = dto.ImageUrl;
        course.CategoryId = dto.CategoryId;
        course.IsFeatured = dto.IsFeatured;
        course.Level = dto.Level;
        course.Language = dto.Language;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SubmitReviewAsync(Guid userId, SubmitReviewDto reviewDto)
    {
        return await _reviewService.AddReviewAsync(userId, new OnlineLearningPlatformAss2.Service.DTOs.Review.ReviewRequest
        {
            CourseId = reviewDto.CourseId,
            Rating = reviewDto.Rating,
            Comment = reviewDto.Comment
        });
    }

    public async Task<IEnumerable<ReviewViewModel>> GetCourseReviewsAsync(Guid courseId)
    {
        var reviews = await _reviewService.GetCourseReviewsAsync(courseId);
        return reviews.Select(r => new ReviewViewModel
        {
            Id = r.Id,
            Username = r.Username,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        });
    }

    public async Task<bool> EnrollUserAsync(Guid userId, Guid courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) return false;

        // Business Rule: Instructor cannot enroll in their own course
        if (course.InstructorId == userId) return false;

        // Business Rule: Cannot enroll if already enrolled
        var isEnrolled = await _context.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
        if (isEnrolled) return false;

        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow,
            Status = "Active"
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<CourseViewModel>> GetEnrolledCoursesAsync(Guid userId)
    {
        var enrollments = await _context.Enrollments
            .Include(e => e.Course)
            .ThenInclude(c => c.Category)
            .Include(e => e.Course.Instructor)
            .Include(e => e.LessonProgresses)
            .Include(e => e.Course.Modules)
            .ThenInclude(m => m.Lessons)
            .Where(e => e.UserId == userId)
            .ToListAsync();

        var enrolledCourses = enrollments.Select(e => {
            var totalLessons = e.Course.Modules.Sum(m => m.Lessons.Count);
            var completedLessons = e.LessonProgresses.Count(lp => lp.IsCompleted);
            var progress = totalLessons > 0 ? (int)((decimal)completedLessons / totalLessons * 100) : 0;

            return new CourseViewModel
            {
                Id = e.Course.Id,
                Title = e.Course.Title,
                Description = e.Course.Description,
                Price = e.Course.Price,
                ImageUrl = e.Course.ImageUrl,
                CategoryName = e.Course.Category.Name,
                InstructorName = e.Course.Instructor.Username,
                Rating = 4.5m,
                StudentCount = _context.Enrollments.Count(en => en.CourseId == e.CourseId),
                EnrollmentDate = e.EnrolledAt,
                Progress = progress
            };
        });

        return enrolledCourses;
    }

    public async Task<IEnumerable<CategoryViewModel>> GetAllCategoriesAsync()
    {
        var categories = await _context.Categories
            .Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CourseCount = _context.Courses.Count(course => course.CategoryId == c.Id)
            })
            .ToListAsync();

        return categories;
    }

    public async Task<CourseLearnViewModel?> GetCourseLearnAsync(Guid enrollmentId)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .ThenInclude(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .Include(e => e.LessonProgresses)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId);

        if (enrollment == null)
            return null;

        var totalLessons = enrollment.Course.Modules.Sum(m => m.Lessons.Count);
        var completedLessons = enrollment.LessonProgresses.Count(lp => lp.IsCompleted);
        var progress = totalLessons > 0 ? (int)((decimal)completedLessons / totalLessons * 100) : 0;

        return new CourseLearnViewModel
        {
            EnrollmentId = enrollment.Id,
            CourseId = enrollment.Course.Id,
            CourseTitle = enrollment.Course.Title,
            CurrentLessonId = enrollment.Course.Modules.FirstOrDefault()?.Lessons.FirstOrDefault()?.Id,
            Progress = progress,
            Modules = enrollment.Course.Modules.Select(m => new ModuleViewModel
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                OrderIndex = m.OrderIndex,
                Lessons = m.Lessons.Select(l => new LessonViewModel
                {
                    Id = l.Id,
                    Title = l.Title,
                    Content = l.Content,
                    VideoUrl = l.VideoUrl,
                    Duration = 15,
                    OrderIndex = l.OrderIndex,
                    IsCompleted = enrollment.LessonProgresses.Any(lp => lp.LessonId == l.Id && lp.IsCompleted)
                }).OrderBy(l => l.OrderIndex).ToList()
            }).OrderBy(m => m.OrderIndex).ToList()
        };
    }

    public async Task<bool> UpdateLessonProgressAsync(Guid enrollmentId, Guid lessonId, bool isCompleted)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.LessonProgresses)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId);

        if (enrollment == null) return false;

        var progress = enrollment.LessonProgresses.FirstOrDefault(p => p.LessonId == lessonId);
        if (progress == null)
        {
            progress = new LessonProgress
            {
                Id = Guid.NewGuid(),
                EnrollmentId = enrollmentId,
                LessonId = lessonId,
                IsCompleted = isCompleted,
                LastAccessedAt = DateTime.UtcNow
            };
            _context.LessonProgresses.Add(progress);
        }
        else
        {
            progress.IsCompleted = isCompleted;
            progress.LastAccessedAt = DateTime.UtcNow;
        }

        // Check if all lessons are completed to update enrollment status
        var course = await _context.Courses
            .Include(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .FirstOrDefaultAsync(c => c.Id == enrollment.CourseId);

        if (course != null)
        {
            var totalLessons = course.Modules.Sum(m => m.Lessons.Count);
            var completedCount = enrollment.LessonProgresses.Count(p => p.IsCompleted) + (isCompleted && !enrollment.LessonProgresses.Any(p => p.LessonId == lessonId && p.IsCompleted) ? 1 : 0);
            
            if (completedCount >= totalLessons)
            {
                enrollment.Status = "Completed";
                enrollment.CompletedAt = DateTime.UtcNow;
                
                // Issue Certificate
                await IssueCertificateAsync(enrollment.UserId, enrollment.CourseId);
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Guid?> GetEnrollmentIdAsync(Guid userId, Guid courseId)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
        return enrollment?.Id;
    }

    // --- Curriculum Management ---

    public async Task<Guid?> AddModuleAsync(Guid courseId, string title, string description, int orderIndex, Guid instructorId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null || course.InstructorId != instructorId) return null;

        var module = new Module
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = title,
            Description = description,
            OrderIndex = orderIndex
        };

        _context.Modules.Add(module);
        await _context.SaveChangesAsync();
        return module.Id;
    }

    public async Task<bool> UpdateModuleAsync(Guid moduleId, string title, string description, int orderIndex, Guid instructorId)
    {
        var module = await _context.Modules
            .Include(m => m.Course)
            .FirstOrDefaultAsync(m => m.Id == moduleId);

        if (module == null || module.Course.InstructorId != instructorId) return false;

        module.Title = title;
        module.Description = description;
        module.OrderIndex = orderIndex;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteModuleAsync(Guid moduleId, Guid instructorId)
    {
        var module = await _context.Modules
            .Include(m => m.Course)
            .FirstOrDefaultAsync(m => m.Id == moduleId);

        if (module == null || module.Course.InstructorId != instructorId) return false;

        _context.Modules.Remove(module);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Guid?> AddLessonAsync(Guid moduleId, string title, string content, string? videoUrl, int orderIndex, Guid instructorId)
    {
        var module = await _context.Modules
            .Include(m => m.Course)
            .FirstOrDefaultAsync(m => m.Id == moduleId);

        if (module == null || module.Course.InstructorId != instructorId) return null;

        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            ModuleId = moduleId,
            Title = title,
            Content = content,
            VideoUrl = videoUrl,
            OrderIndex = orderIndex
        };

        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();
        return lesson.Id;
    }

    public async Task<bool> UpdateLessonAsync(Guid lessonId, string title, string content, string? videoUrl, int orderIndex, Guid instructorId)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Module)
            .ThenInclude(m => m.Course)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null || lesson.Module.Course.InstructorId != instructorId) return false;

        lesson.Title = title;
        lesson.Content = content;
        lesson.VideoUrl = videoUrl;
        lesson.OrderIndex = orderIndex;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteLessonAsync(Guid lessonId, Guid instructorId)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Module)
            .ThenInclude(m => m.Course)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null || lesson.Module.Course.InstructorId != instructorId) return false;

        _context.Lessons.Remove(lesson);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetInstructorEarningsAsync(Guid instructorId)
    {
        var totalAmount = await _context.Orders
            .Where(o => o.Course.InstructorId == instructorId && o.Status == "Completed")
            .SumAsync(o => o.TotalAmount);

        return totalAmount * 0.70m; // 70% instructor revenue share
    }

    public async Task<bool> IssueCertificateAsync(Guid userId, Guid courseId)
    {
        // Check if certificate already exists
        var exists = await _context.Certificates.AnyAsync(c => c.UserId == userId && c.CourseId == courseId);
        if (exists) return true;

        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) return false;

        var certificate = new Certificate
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = courseId,
            IssuedAt = DateTime.UtcNow,
            CertificateUrl = $"/Certificates/View/{Guid.NewGuid()}" // Synthetic URL for now
        };

        _context.Certificates.Add(certificate);
        await _context.SaveChangesAsync();
        return true;
    }
}
