using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.DTOs.Category;
using OnlineLearningPlatformAss2.Data.Entities;
using OnlineLearningPlatformAss2.Data.Repositories.Interfaces;

namespace OnlineLearningPlatformAss2.Service.Services;

public class CourseService(
    ICourseRepository courseRepository,
    IEnrollmentRepository enrollmentRepository,
    IReviewService reviewService,
    ITranscriptService transcriptService) : ICourseService
{
    public async Task<IEnumerable<CourseViewModel>> GetFeaturedCoursesAsync(int? limit = null)
    {
        var courses = await courseRepository.GetFeaturedCoursesAsync(limit ?? 6);
        var viewModels = new List<CourseViewModel>();
        
        foreach (var c in courses)
        {
            viewModels.Add(new CourseViewModel
            {
                Id = c.CourseId,
                Title = c.Title,
                Description = c.Description ?? string.Empty,
                Price = c.Price,
                ImageUrl = c.ImageUrl,
                CategoryName = c.Category.Name,
                InstructorName = c.Instructor.Username,
                Rating = c.CourseReviews.Any() ? (decimal)c.CourseReviews.Average(r => r.Rating) : 0m,
                StudentCount = await courseRepository.GetEnrollmentCountAsync(c.CourseId),
                IsFeatured = true
            });
        }
        
        return viewModels;
    }

    public async Task<IEnumerable<CourseViewModel>> GetAllCoursesAsync(string? searchTerm = null, Guid? categoryId = null, int? limit = null)
    {
        var courses = await courseRepository.GetCoursesAsync(searchTerm, categoryId, limit);
        var viewModels = new List<CourseViewModel>();
        
        foreach (var c in courses)
        {
            viewModels.Add(new CourseViewModel
            {
                Id = c.CourseId,
                Title = c.Title,
                Description = c.Description ?? string.Empty,
                Price = c.Price,
                ImageUrl = c.ImageUrl,
                CategoryName = c.Category.Name,
                InstructorName = c.Instructor.Username,
                Rating = c.CourseReviews.Any() ? (decimal)c.CourseReviews.Average(r => r.Rating) : 0m,
                StudentCount = await courseRepository.GetEnrollmentCountAsync(c.CourseId),
                IsFeatured = false
            });
        }
        
        return viewModels;
    }

    public async Task<CourseDetailViewModel?> GetCourseDetailsAsync(Guid id, Guid? userId = null)
    {
        var course = await courseRepository.GetByIdWithDetailsAsync(id);
        if (course == null || course.Status != "Published")
            return null;

        var isEnrolled = false;
        var isInWishlist = false;
        if (userId.HasValue)
        {
            isEnrolled = await enrollmentRepository.IsEnrolledAsync(userId.Value, id);
            isInWishlist = await courseRepository.IsInWishlistAsync(userId.Value, id);
        }

        var studentCount = await courseRepository.GetEnrollmentCountAsync(id);
        var ratingSummary = await reviewService.GetRatingSummaryAsync(id);

        return new CourseDetailViewModel
        {
            Id = course.CourseId,
            Title = course.Title,
            Description = course.Description ?? string.Empty,
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
                Id = m.ModuleId,
                Title = m.Title,
                Description = m.Description ?? string.Empty,
                OrderIndex = m.OrderIndex,
                Lessons = m.Lessons.Select(l => new LessonViewModel
                {
                    Id = l.LessonId,
                    Title = l.Title,
                    Duration = l.Duration ?? 15,
                    OrderIndex = l.OrderIndex,
                    IsPreview = l.OrderIndex <= 2
                }).OrderBy(l => l.OrderIndex).ToList()
            }).OrderBy(m => m.OrderIndex).ToList(),
            Reviews = (await reviewService.GetCourseReviewsAsync(id)).Select(r => new ReviewViewModel 
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
        var existing = await courseRepository.GetWishlistItemAsync(userId, courseId);

        if (existing != null)
        {
            await courseRepository.RemoveWishlistAsync(existing);
        }
        else
        {
            await courseRepository.AddWishlistAsync(new Wishlist
            {
                WishlistId = Guid.NewGuid(),
                UserId = userId,
                CourseId = courseId,
                AddedAt = DateTime.UtcNow
            });
        }

        await courseRepository.SaveChangesAsync();
        return existing == null;
    }

    public async Task<IEnumerable<CourseViewModel>> GetWishlistAsync(Guid userId)
    {
        var wishlists = await courseRepository.GetUserWishlistAsync(userId);
        return wishlists.Select(w => new CourseViewModel
        {
            Id = w.Course.CourseId,
            Title = w.Course.Title,
            Description = w.Course.Description ?? string.Empty,
            Price = w.Course.Price,
            ImageUrl = w.Course.ImageUrl,
            CategoryName = w.Course.Category.Name,
            InstructorName = w.Course.Instructor.Username,
            Rating = 4.5m,
            StudentCount = 0,
            IsInWishlist = true
        });
    }

    public async Task<IEnumerable<CourseViewModel>> GetInstructorCoursesAsync(Guid instructorId)
    {
        var courses = await courseRepository.GetInstructorCoursesAsync(instructorId);
        var viewModels = new List<CourseViewModel>();
        
        foreach (var c in courses)
        {
            viewModels.Add(new CourseViewModel
            {
                Id = c.CourseId,
                Title = c.Title,
                Description = c.Description ?? string.Empty,
                Price = c.Price,
                ImageUrl = c.ImageUrl,
                CategoryName = c.Category.Name,
                InstructorName = c.Instructor.Username,
                Rating = c.CourseReviews.Any() ? (decimal)c.CourseReviews.Average(r => r.Rating) : 0,
                StudentCount = await courseRepository.GetEnrollmentCountAsync(c.CourseId),
                IsFeatured = c.IsFeatured,
                Status = c.Status,
                RejectionReason = c.RejectionReason
            });
        }
        
        return viewModels;
    }

    public async Task<bool> UpdateCourseAsync(Guid courseId, CourseUpdateDto dto, Guid instructorId)
    {
        var course = await courseRepository.GetByIdAsync(courseId);
        if (course == null || course.InstructorId != instructorId) return false;

        course.Title = dto.Title;
        course.Description = dto.Description;
        course.Price = dto.Price;
        course.ImageUrl = dto.ImageUrl;
        course.CategoryId = dto.CategoryId;
        course.IsFeatured = dto.IsFeatured;
        course.Level = dto.Level;
        course.Language = dto.Language;

        await courseRepository.UpdateAsync(course);
        await courseRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SubmitReviewAsync(Guid userId, SubmitReviewDto reviewDto)
    {
        return await reviewService.AddReviewAsync(userId, new OnlineLearningPlatformAss2.Service.DTOs.Review.ReviewRequest
        {
            CourseId = reviewDto.CourseId,
            Rating = reviewDto.Rating,
            Comment = reviewDto.Comment
        });
    }

    public async Task<IEnumerable<ReviewViewModel>> GetCourseReviewsAsync(Guid courseId)
    {
        var reviews = await reviewService.GetCourseReviewsAsync(courseId);
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
        var course = await courseRepository.GetByIdAsync(courseId);
        if (course == null) return false;

        if (course.InstructorId == userId) return false;

        var isEnrolled = await enrollmentRepository.IsEnrolledAsync(userId, courseId);
        if (isEnrolled) return false;

        var enrollment = new Enrollment
        {
            EnrollmentId = Guid.NewGuid(),
            UserId = userId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow,
            Status = "Active"
        };

        await enrollmentRepository.AddAsync(enrollment);
        await enrollmentRepository.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<CourseViewModel>> GetEnrolledCoursesAsync(Guid userId)
    {
        var enrollments = await enrollmentRepository.GetStudentEnrollmentsDetailedAsync(userId);
        
        return enrollments.Select(e => {
            var totalLessons = e.Course.Modules.Sum(m => m.Lessons.Count);
            var completedLessons = e.LessonProgresses.Count(lp => lp.IsCompleted);
            var progress = totalLessons > 0 ? (int)((decimal)completedLessons / totalLessons * 100) : 0;

            return new CourseViewModel
            {
                Id = e.Course.CourseId,
                Title = e.Course.Title,
                Description = e.Course.Description ?? string.Empty,
                Price = e.Course.Price,
                ImageUrl = e.Course.ImageUrl,
                CategoryName = e.Course.Category.Name,
                InstructorName = e.Course.Instructor.Username,
                Rating = 4.5m,
                StudentCount = 0,
                EnrollmentDate = e.EnrolledAt,
                Progress = progress
            };
        });
    }

    public async Task<IEnumerable<CategoryViewModel>> GetAllCategoriesAsync()
    {
        var categories = await courseRepository.GetAllCategoriesAsync();
        var viewModels = new List<CategoryViewModel>();
        
        foreach (var c in categories)
        {
            viewModels.Add(new CategoryViewModel
            {
                Id = c.CategoryId,
                Name = c.Name,
                Description = c.Description ?? string.Empty,
                CourseCount = await courseRepository.GetEnrollmentCountAsync(c.CategoryId)
            });
        }
        
        return viewModels;
    }

    public async Task<CourseLearnViewModel?> GetCourseLearnAsync(Guid enrollmentId)
    {
        var enrollment = await enrollmentRepository.GetByIdWithDetailsAsync(enrollmentId);
        if (enrollment == null)
            return null;

        var totalLessons = enrollment.Course.Modules.Sum(m => m.Lessons.Count);
        var completedLessons = enrollment.LessonProgresses.Count(lp => lp.IsCompleted);
        var progress = totalLessons > 0 ? (int)((decimal)completedLessons / totalLessons * 100) : 0;

        var certificate = await courseRepository.GetCertificateByEnrollmentIdAsync(enrollmentId);

        return new CourseLearnViewModel
        {
            EnrollmentId = enrollment.EnrollmentId,
            CourseId = enrollment.Course.CourseId,
            CourseTitle = enrollment.Course.Title,
            CurrentLessonId = enrollment.LastViewedLessonId ?? enrollment.Course.Modules.FirstOrDefault()?.Lessons.FirstOrDefault()?.LessonId,
            Progress = progress,
            CertificateId = certificate?.CertificateId,
            Modules = enrollment.Course.Modules.Select(m => new ModuleViewModel
            {
                Id = m.ModuleId,
                Title = m.Title,
                Description = m.Description ?? string.Empty,
                OrderIndex = m.OrderIndex,
                Lessons = m.Lessons.Select(l => 
                {
                    var progress = enrollment.LessonProgresses.FirstOrDefault(lp => lp.LessonId == l.LessonId);
                    return new LessonViewModel
                    {
                        Id = l.LessonId,
                        Title = l.Title,
                        Content = l.Content ?? string.Empty,
                        VideoUrl = l.VideoUrl,
                        Duration = l.Duration ?? 15,
                        OrderIndex = l.OrderIndex,
                        IsCompleted = progress?.IsCompleted ?? false,
                        LastWatchedPosition = progress?.LastWatchedPosition,
                        AiSummary = progress?.AiSummary,
                        Transcript = progress?.Transcript
                    };
                }).OrderBy(l => l.OrderIndex).ToList()
            }).OrderBy(m => m.OrderIndex).ToList()
        };
    }

    public async Task<bool> UpdateLastViewedLessonAsync(Guid enrollmentId, Guid lessonId)
    {
        var enrollment = await enrollmentRepository.GetByIdAsync(enrollmentId);
        if (enrollment == null) return false;

        enrollment.LastViewedLessonId = lessonId;
        await enrollmentRepository.UpdateAsync(enrollment);
        await enrollmentRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SaveLessonAiDataAsync(Guid enrollmentId, Guid lessonId, string transcript, string summary)
    {
        var progress = await enrollmentRepository.GetLessonProgressAsync(enrollmentId, lessonId);
        if (progress == null)
        {
            progress = new LessonProgress
            {
                ProgressId = Guid.NewGuid(),
                EnrollmentId = enrollmentId,
                LessonId = lessonId,
                Transcript = transcript,
                AiSummary = summary,
                LastAccessedAt = DateTime.UtcNow
            };
            await enrollmentRepository.AddLessonProgressAsync(progress);
        }
        else
        {
            progress.Transcript = transcript;
            progress.AiSummary = summary; 
            progress.LastAccessedAt = DateTime.UtcNow;
            await enrollmentRepository.UpdateLessonProgressAsync(progress);
        }
        await enrollmentRepository.SaveChangesAsync();
        return true;
    }

    public async Task<string> GetLessonTranscriptAsync(Guid enrollmentId, Guid lessonId)
    {
        var progress = await enrollmentRepository.GetLessonProgressAsync(enrollmentId, lessonId);
        
        if (progress != null && !string.IsNullOrEmpty(progress.Transcript))
        {
            return progress.Transcript;
        }

        var lesson = await courseRepository.GetLessonByIdAsync(lessonId);
        if (lesson == null || string.IsNullOrEmpty(lesson.VideoUrl))
        {
            return string.Empty;
        }

        string transcript = string.Empty;
        try 
        {
            transcript = await transcriptService.GenerateTranscriptFromVideoAsync(lesson.VideoUrl);
        }
        catch 
        {
            return string.Empty;
        }

        if (string.IsNullOrEmpty(transcript))
        {
             return string.Empty;
        }

        // Use the new method to save
        await SaveLessonAiDataAsync(enrollmentId, lessonId, transcript, string.Empty);
        return transcript;
    }

    public async Task<bool> UpdateVideoProgressAsync(Guid enrollmentId, Guid lessonId, int position)
    {
        var progress = await enrollmentRepository.GetLessonProgressAsync(enrollmentId, lessonId);
        if (progress == null)
        {
            progress = new LessonProgress
            {
                ProgressId = Guid.NewGuid(),
                EnrollmentId = enrollmentId,
                LessonId = lessonId,
                LastWatchedPosition = position,
                LastAccessedAt = DateTime.UtcNow
            };
            await enrollmentRepository.AddLessonProgressAsync(progress);
        }
        else
        {
            progress.LastWatchedPosition = position;
            progress.LastAccessedAt = DateTime.UtcNow;
            await enrollmentRepository.UpdateLessonProgressAsync(progress);
        }
        await enrollmentRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateLessonProgressAsync(Guid enrollmentId, Guid lessonId, bool isCompleted)
    {
        var enrollment = await enrollmentRepository.GetByIdWithDetailsAsync(enrollmentId);
        if (enrollment == null) return false;

        var progress = await enrollmentRepository.GetLessonProgressAsync(enrollmentId, lessonId);
        if (progress == null)
        {
            progress = new LessonProgress
            {
                ProgressId = Guid.NewGuid(),
                EnrollmentId = enrollmentId,
                LessonId = lessonId,
                IsCompleted = isCompleted
            };
            await enrollmentRepository.AddLessonProgressAsync(progress);
        }
        else
        {
            progress.IsCompleted = isCompleted;
            await enrollmentRepository.UpdateLessonProgressAsync(progress);
        }

        var course = await courseRepository.GetByIdWithDetailsAsync(enrollment.CourseId);
        if (course != null)
        {
            var totalLessons = course.Modules.Sum(m => m.Lessons.Count);
            var completedCount = enrollment.LessonProgresses.Count(p => p.IsCompleted) + 
                (isCompleted && !enrollment.LessonProgresses.Any(p => p.LessonId == lessonId && p.IsCompleted) ? 1 : 0);
            
            if (completedCount >= totalLessons)
            {
                enrollment.Status = "Completed";
                enrollment.CompletedAt = DateTime.UtcNow;
                await enrollmentRepository.UpdateAsync(enrollment);
                await IssueCertificateAsync(enrollment.UserId, enrollment.CourseId);
            }
        }

        await enrollmentRepository.SaveChangesAsync();
        return true;
    }

    public async Task<Guid?> GetEnrollmentIdAsync(Guid userId, Guid courseId)
    {
        var enrollment = await enrollmentRepository.GetByUserAndCourseAsync(userId, courseId);
        return enrollment?.EnrollmentId;
    }

    // --- Curriculum Management ---

    public async Task<Guid?> AddModuleAsync(Guid courseId, string title, string description, int orderIndex, Guid instructorId)
    {
        var course = await courseRepository.GetByIdAsync(courseId);
        if (course == null || course.InstructorId != instructorId) return null;

        var module = new Module
        {
            ModuleId = Guid.NewGuid(),
            CourseId = courseId,
            Title = title,
            Description = description,
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow
        };

        await courseRepository.AddModuleAsync(module);
        await courseRepository.SaveChangesAsync();
        return module.ModuleId;
    }

    public async Task<bool> UpdateModuleAsync(Guid moduleId, string title, string description, int orderIndex, Guid instructorId)
    {
        var module = await courseRepository.GetModuleByIdAsync(moduleId);
        if (module == null || module.Course.InstructorId != instructorId) return false;

        module.Title = title;
        module.Description = description;
        module.OrderIndex = orderIndex;

        await courseRepository.UpdateModuleAsync(module);
        await courseRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteModuleAsync(Guid moduleId, Guid instructorId)
    {
        var module = await courseRepository.GetModuleByIdAsync(moduleId);
        if (module == null || module.Course.InstructorId != instructorId) return false;

        await courseRepository.RemoveModuleAsync(module);
        await courseRepository.SaveChangesAsync();
        return true;
    }

    public async Task<Guid?> AddLessonAsync(Guid moduleId, string title, string content, string? videoUrl, int orderIndex, Guid instructorId)
    {
        var module = await courseRepository.GetModuleByIdAsync(moduleId);
        if (module == null || module.Course.InstructorId != instructorId) return null;

        var lesson = new Lesson
        {
            LessonId = Guid.NewGuid(),
            ModuleId = moduleId,
            Title = title,
            Content = content,
            VideoUrl = videoUrl,
            Type = "Video",
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow
        };

        await courseRepository.AddLessonAsync(lesson);
        await courseRepository.SaveChangesAsync();
        return lesson.LessonId;
    }

    public async Task<bool> UpdateLessonAsync(Guid lessonId, string title, string content, string? videoUrl, int orderIndex, Guid instructorId)
    {
        var lesson = await courseRepository.GetLessonByIdAsync(lessonId);
        if (lesson == null || lesson.Module.Course.InstructorId != instructorId) return false;

        lesson.Title = title;
        lesson.Content = content;
        lesson.VideoUrl = videoUrl;
        lesson.OrderIndex = orderIndex;

        await courseRepository.UpdateLessonAsync(lesson);
        await courseRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteLessonAsync(Guid lessonId, Guid instructorId)
    {
        var lesson = await courseRepository.GetLessonByIdAsync(lessonId);
        if (lesson == null || lesson.Module.Course.InstructorId != instructorId) return false;

        await courseRepository.RemoveLessonAsync(lesson);
        await courseRepository.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetInstructorEarningsAsync(Guid instructorId)
    {
        var totalAmount = await courseRepository.GetInstructorEarningsAsync(instructorId);
        return totalAmount * 0.70m;
    }

    public async Task<bool> IssueCertificateAsync(Guid userId, Guid courseId)
    {
        var enrollment = await enrollmentRepository.GetByUserAndCourseAsync(userId, courseId);
        if (enrollment == null) return false;

        var exists = await courseRepository.CertificateExistsAsync(enrollment.EnrollmentId);
        if (exists) return true;

        var certificateId = Guid.NewGuid();
        var certificate = new Certificate
        {
            CertificateId = certificateId,
            EnrollmentId = enrollment.EnrollmentId,
            SerialNumber = $"CERT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            IssueDate = DateTime.UtcNow,
            PdfUrl = $"/Certificates/View/{certificateId}"
        };

        await courseRepository.AddCertificateAsync(certificate);
        await courseRepository.SaveChangesAsync();
        return true;
    }

    public async Task<CertificateViewModel?> GetCertificateAsync(Guid certificateId)
    {
        var cert = await courseRepository.GetCertificateByIdAsync(certificateId);
        if (cert == null) return null;

        return new CertificateViewModel
        {
            CertificateId = cert.CertificateId,
            Username = cert.Enrollment.User.Username,
            CourseTitle = cert.Enrollment.Course.Title,
            InstructorName = cert.Enrollment.Course.Instructor.Username,
            IssuedDate = cert.IssueDate,
            VerificationCode = cert.CertificateId.ToString().Split('-')[0].ToUpper()
        };
    }
}
