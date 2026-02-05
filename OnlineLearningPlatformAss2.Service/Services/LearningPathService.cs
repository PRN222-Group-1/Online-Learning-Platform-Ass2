using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.LearningPath;
using OnlineLearningPlatformAss2.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace OnlineLearningPlatformAss2.Service.Services;

public class LearningPathService : ILearningPathService
{
    private readonly OnlineLearningContext _context;

    public LearningPathService(OnlineLearningContext context)
    {
        _context = context;
    }

    public async Task<LearningPathViewModel?> GetLearningPathDetailsAsync(Guid id)
    {
        var path = await _context.LearningPaths
            .Include(lp => lp.PathCourses)
            .ThenInclude(pc => pc.Course)
            .ThenInclude(c => c.Category)
            .Include(lp => lp.PathCourses)
            .ThenInclude(pc => pc.Course.Instructor)
            .FirstOrDefaultAsync(lp => lp.Id == id);

        if (path == null)
            return null;

        return new LearningPathViewModel
        {
            Id = path.Id,
            Title = path.Title,
            Description = path.Description,
            Price = path.Price,
            Status = path.Status,
            IsCustomPath = path.IsCustomPath,
            CreatedAt = path.CreatedAt,
            Courses = path.PathCourses.OrderBy(pc => pc.OrderIndex).Select(pc => new CourseInPathViewModel
            {
                CourseId = pc.Course.Id,
                Title = pc.Course.Title,
                Description = pc.Course.Description,
                ImageUrl = pc.Course.ImageUrl,
                OrderIndex = pc.OrderIndex,
                InstructorName = pc.Course.Instructor.Username,
                CategoryName = pc.Course.Category.Name
            }).ToList()
        };
    }

    public async Task<IEnumerable<LearningPathViewModel>> GetFeaturedLearningPathsAsync(Guid? userId = null)
    {
        var paths = await _context.LearningPaths
            .Include(lp => lp.PathCourses)
            .Where(lp => lp.Status == "Published")
            .Take(4)
            .ToListAsync();

        var enrolledPathIds = new HashSet<Guid>();
        if (userId.HasValue)
        {
            enrolledPathIds = new HashSet<Guid>(await _context.UserLearningPathEnrollments
                .Where(e => e.UserId == userId.Value)
                .Select(e => e.PathId)
                .ToListAsync());
        }

        return paths.Select(lp => new LearningPathViewModel
        {
            Id = lp.Id,
            Title = lp.Title,
            Description = lp.Description,
            Price = lp.Price,
            Status = lp.Status,
            IsCustomPath = lp.IsCustomPath,
            CourseCount = lp.PathCourses.Count(),
            IsEnrolled = enrolledPathIds.Contains(lp.Id),
            CreatedAt = lp.CreatedAt
        });
    }

    public async Task<IEnumerable<LearningPathViewModel>> GetPublishedPathsAsync(Guid? userId = null)
    {
        var paths = await _context.LearningPaths
            .Include(lp => lp.PathCourses)
            .Where(lp => lp.Status == "Published")
            .OrderByDescending(lp => lp.CreatedAt)
            .ToListAsync();

        var enrolledPathIds = new HashSet<Guid>();
        if (userId.HasValue)
        {
            enrolledPathIds = new HashSet<Guid>(await _context.UserLearningPathEnrollments
                .Where(e => e.UserId == userId.Value)
                .Select(e => e.PathId)
                .ToListAsync());
        }

        return paths.Select(lp => new LearningPathViewModel
        {
            Id = lp.Id,
            Title = lp.Title,
            Description = lp.Description,
            Price = lp.Price,
            Status = lp.Status,
            IsCustomPath = lp.IsCustomPath,
            CourseCount = lp.PathCourses.Count(),
            IsEnrolled = enrolledPathIds.Contains(lp.Id),
            CreatedAt = lp.CreatedAt
        });
    }

    public async Task<IEnumerable<UserLearningPathWithProgressDto>> GetUserEnrolledPathsAsync(Guid userId)
    {
        var enrollments = await _context.UserLearningPathEnrollments
            .Include(ulpe => ulpe.LearningPath)
            .ThenInclude(lp => lp.PathCourses)
            .Where(ulpe => ulpe.UserId == userId)
            .ToListAsync();

        var result = new List<UserLearningPathWithProgressDto>();

        foreach (var ulpe in enrollments)
        {
            var courseIds = ulpe.LearningPath.PathCourses.Select(pc => pc.CourseId).ToList();
            var courseEnrollments = await _context.Enrollments
                .Where(e => e.UserId == userId && courseIds.Contains(e.CourseId))
                .ToListAsync();

            var completedCourses = courseEnrollments.Count(e => e.Status == "Completed");
            var totalCourses = courseIds.Count;
            var progress = totalCourses > 0 ? (decimal)completedCourses / totalCourses * 100 : 0;

            result.Add(new UserLearningPathWithProgressDto
            {
                Id = ulpe.Id,
                PathId = ulpe.PathId,
                PathTitle = ulpe.LearningPath.Title,
                PathDescription = ulpe.LearningPath.Description,
                EnrolledAt = ulpe.EnrolledAt,
                Status = ulpe.Status,
                TotalCourses = totalCourses,
                CompletedCourses = completedCourses,
                Progress = progress
            });
        }

        return result;
    }

    public async Task<UserLearningPathWithProgressDto?> GetUserPathProgressAsync(Guid userId, Guid pathId)
    {
        var enrollment = await _context.UserLearningPathEnrollments
            .Include(ulpe => ulpe.LearningPath)
            .ThenInclude(lp => lp.PathCourses)
            .FirstOrDefaultAsync(ulpe => ulpe.UserId == userId && ulpe.PathId == pathId);

        if (enrollment == null)
            return null;

        var courseIds = enrollment.LearningPath.PathCourses.Select(pc => pc.CourseId).ToList();
        var courseEnrollments = await _context.Enrollments
            .Where(e => e.UserId == userId && courseIds.Contains(e.CourseId))
            .ToListAsync();

        var completedCourses = courseEnrollments.Count(e => e.Status == "Completed");
        var totalCourses = courseIds.Count;
        var progress = totalCourses > 0 ? (decimal)completedCourses / totalCourses * 100 : 0;

        return new UserLearningPathWithProgressDto
        {
            Id = enrollment.Id,
            PathId = enrollment.PathId,
            PathTitle = enrollment.LearningPath.Title,
            PathDescription = enrollment.LearningPath.Description,
            EnrolledAt = enrollment.EnrolledAt,
            CompletedAt = enrollment.CompletedAt,
            Status = enrollment.Status,
            TotalCourses = totalCourses,
            CompletedCourses = completedCourses,
            Progress = progress
        };
    }

    public async Task<LearningPathDetailsWithProgressDto?> GetPathDetailsWithProgressAsync(Guid pathId, Guid? userId = null)
    {
        var path = await _context.LearningPaths
            .Include(lp => lp.PathCourses)
            .ThenInclude(pc => pc.Course)
            .ThenInclude(c => c.Category)
            .Include(lp => lp.PathCourses)
            .ThenInclude(pc => pc.Course.Instructor)
            .FirstOrDefaultAsync(lp => lp.Id == pathId);

        if (path == null)
            return null;

        bool isEnrolled = false;
        decimal totalProgress = 0m;
        
        var coursesWithProgress = new List<PathCourseWithProgressDto>();
        var pathCourses = path.PathCourses.OrderBy(pc => pc.OrderIndex).ToList();

        if (userId.HasValue)
        {
            isEnrolled = await _context.UserLearningPathEnrollments
                .AnyAsync(ulpe => ulpe.UserId == userId.Value && ulpe.PathId == pathId);

            var enrollments = await _context.Enrollments
                .Include(e => e.LessonProgresses)
                .Where(e => e.UserId == userId.Value)
                .ToListAsync();

            int completedCourses = 0;
            for (int i = 0; i < pathCourses.Count; i++)
            {
                var pc = pathCourses[i];
                var enrollment = enrollments.FirstOrDefault(e => e.CourseId == pc.CourseId);
                
                decimal courseProgress = 0m;
                bool isCompleted = false;

                if (enrollment != null)
                {
                    var totalLessons = await _context.Lessons.CountAsync(l => _context.Modules.Any(m => m.CourseId == pc.CourseId && m.Id == l.ModuleId));
                    var completedLessons = enrollment.LessonProgresses.Count(p => p.IsCompleted);
                    courseProgress = totalLessons > 0 ? (decimal)completedLessons / totalLessons * 100 : 0;
                    isCompleted = enrollment.Status == "Completed";
                    if (isCompleted) completedCourses++;
                }

                coursesWithProgress.Add(new PathCourseWithProgressDto
                {
                    CourseId = pc.Course.Id,
                    Title = pc.Course.Title,
                    Description = pc.Course.Description,
                    ImageUrl = pc.Course.ImageUrl,
                    OrderIndex = pc.OrderIndex,
                    InstructorName = pc.Course.Instructor.Username,
                    Duration = 180,
                    Level = "All Levels",
                    IsCompleted = isCompleted,
                    IsCurrentCourse = isEnrolled && !isCompleted && (i == 0 || coursesWithProgress[i-1].IsCompleted),
                    IsLocked = !isEnrolled,
                    Progress = courseProgress
                });
            }
            
            totalProgress = pathCourses.Count > 0 ? (decimal)completedCourses / pathCourses.Count * 100 : 0;
        }
        else
        {
            coursesWithProgress = pathCourses.Select(pc => new PathCourseWithProgressDto
            {
                CourseId = pc.Course.Id,
                Title = pc.Course.Title,
                Description = pc.Course.Description,
                ImageUrl = pc.Course.ImageUrl,
                OrderIndex = pc.OrderIndex,
                InstructorName = pc.Course.Instructor.Username,
                Duration = 180,
                Level = "All Levels",
                IsCompleted = false,
                IsCurrentCourse = false,
                IsLocked = true,
                Progress = 0m
            }).ToList();
        }

        return new LearningPathDetailsWithProgressDto
        {
            Id = path.Id,
            Title = path.Title,
            Description = path.Description,
            Price = path.Price,
            IsCustomPath = path.IsCustomPath,
            IsEnrolled = isEnrolled,
            Progress = totalProgress,
            WhatYouWillLearn = new List<string> { "Foundational concepts", "Advanced techniques", "Practical projects", "Industry best practices" },
            Prerequisites = new List<string> { "Basic computer literacy", "Interest in the subject" },
            Courses = coursesWithProgress
        };
    }
}
