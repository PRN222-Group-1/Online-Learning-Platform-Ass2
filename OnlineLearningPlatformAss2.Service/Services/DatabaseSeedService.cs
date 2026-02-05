using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace OnlineLearningPlatformAss2.Service.Services;

public class DatabaseSeedService
{
    private readonly OnlineLearningContext _context;

    public DatabaseSeedService(OnlineLearningContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        
        // Seed Roles
        await SeedRolesAsync();
        
        // Seed Categories
        await SeedCategoriesAsync();

        // Seed Assessment Questions
        await SeedAssessmentQuestionsAsync();

        // Seed Sample Users (Instructors)
        await SeedInstructorsAsync();

        // Seed Admin User
        await SeedAdminAsync();
        
        // Seed Comprehensive Course Data
        await SeedComprehensiveCoursesAsync();

        // Seed Learning Paths
        await SeedLearningPathsAsync();

        // Seed Sample Users and Orders
        await SeedSampleUsersAndOrdersAsync();

        // Seed Quizzes for some lessons
        await SeedQuizzesAsync();
    }

    private async Task SeedRolesAsync()
    {
        if (!await _context.Roles.AnyAsync())
        {
            var roles = new[]
            {
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    Description = "Administrator with full system access",
                    CreatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Instructor",
                    Description = "Course instructor who can create and manage courses",
                    CreatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "User",
                    Description = "Regular learner user",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.Roles.AddRangeAsync(roles);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedCategoriesAsync()
    {
        if (!await _context.Categories.AnyAsync())
        {
            var categories = new[]
            {
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Web Development",
                    Description = "Learn web development technologies including HTML, CSS, JavaScript, and frameworks",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Data Science",
                    Description = "Master data analysis, machine learning, and statistical modeling",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Design",
                    Description = "UI/UX design, graphic design, and creative skills",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Business",
                    Description = "Business strategy, management, and entrepreneurship",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Marketing",
                    Description = "Digital marketing, SEO, social media, and advertising",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Mobile Development",
                    Description = "iOS, Android, and cross-platform mobile app development",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "DevOps",
                    Description = "Cloud computing, CI/CD, and infrastructure management",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Cybersecurity",
                    Description = "Information security, ethical hacking, and security best practices",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedAssessmentQuestionsAsync()
    {
        if (!await _context.AssessmentQuestions.AnyAsync())
        {
            var questions = new[]
            {
                new AssessmentQuestion
                {
                    Id = Guid.NewGuid(),
                    QuestionText = "What is your current experience level with programming?",
                    QuestionType = "SingleChoice",
                    OrderIndex = 1,
                    IsActive = true
                },
                new AssessmentQuestion
                {
                    Id = Guid.NewGuid(),
                    QuestionText = "Which area interests you most?",
                    QuestionType = "SingleChoice",
                    OrderIndex = 2,
                    IsActive = true
                },
                new AssessmentQuestion
                {
                    Id = Guid.NewGuid(),
                    QuestionText = "What is your primary goal for learning?",
                    QuestionType = "SingleChoice",
                    OrderIndex = 3,
                    IsActive = true
                },
                new AssessmentQuestion
                {
                    Id = Guid.NewGuid(),
                    QuestionText = "How much time can you dedicate to learning per week?",
                    QuestionType = "SingleChoice",
                    OrderIndex = 4,
                    IsActive = true
                },
                new AssessmentQuestion
                {
                    Id = Guid.NewGuid(),
                    QuestionText = "Which learning style works best for you?",
                    QuestionType = "SingleChoice",
                    OrderIndex = 5,
                    IsActive = true
                }
            };

            await _context.AssessmentQuestions.AddRangeAsync(questions);
            await _context.SaveChangesAsync();

            // Add options for each question
            await SeedAssessmentOptionsAsync(questions);
        }
    }

    private async Task SeedAssessmentOptionsAsync(AssessmentQuestion[] questions)
    {
        var options = new List<AssessmentOption>();

        // Question 1: Programming experience
        var q1 = questions[0];
        options.AddRange(new[]
        {
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q1.Id,
                OptionText = "Complete beginner - I've never coded before",
                SkillLevel = "Beginner",
                Category = "Programming"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q1.Id,
                OptionText = "Some experience - I've done basic tutorials",
                SkillLevel = "Beginner",
                Category = "Programming"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q1.Id,
                OptionText = "Intermediate - I can build simple applications",
                SkillLevel = "Intermediate",
                Category = "Programming"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q1.Id,
                OptionText = "Advanced - I'm comfortable with complex projects",
                SkillLevel = "Advanced",
                Category = "Programming"
            }
        });

        // Question 2: Interest area
        var q2 = questions[1];
        options.AddRange(new[]
        {
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q2.Id,
                OptionText = "Web Development - Building websites and web applications",
                SkillLevel = "None",
                Category = "Web Development"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q2.Id,
                OptionText = "Data Science - Analyzing data and machine learning",
                SkillLevel = "None",
                Category = "Data Science"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q2.Id,
                OptionText = "Design - UI/UX and graphic design",
                SkillLevel = "None",
                Category = "Design"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q2.Id,
                OptionText = "Business - Management and entrepreneurship",
                SkillLevel = "None",
                Category = "Business"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q2.Id,
                OptionText = "Mobile Development - iOS and Android apps",
                SkillLevel = "None",
                Category = "Mobile Development"
            }
        });

        // Question 3: Learning goal
        var q3 = questions[2];
        options.AddRange(new[]
        {
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q3.Id,
                OptionText = "Career change - I want to switch to a new field",
                SkillLevel = "None",
                Category = "Career Development"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q3.Id,
                OptionText = "Skill improvement - I want to advance in my current role",
                SkillLevel = "None",
                Category = "Professional Growth"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q3.Id,
                OptionText = "Personal interest - Learning for fun and curiosity",
                SkillLevel = "None",
                Category = "Personal Development"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q3.Id,
                OptionText = "Academic requirements - For school or certification",
                SkillLevel = "None",
                Category = "Academic"
            }
        });

        // Question 4: Time commitment
        var q4 = questions[3];
        options.AddRange(new[]
        {
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q4.Id,
                OptionText = "1-3 hours - I have limited time but want to learn",
                SkillLevel = "None",
                Category = "Time Management"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q4.Id,
                OptionText = "4-7 hours - I can dedicate regular time to learning",
                SkillLevel = "None",
                Category = "Time Management"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q4.Id,
                OptionText = "8-15 hours - Learning is a high priority for me",
                SkillLevel = "None",
                Category = "Time Management"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q4.Id,
                OptionText = "15+ hours - I'm fully committed to intensive learning",
                SkillLevel = "None",
                Category = "Time Management"
            }
        });

        // Question 5: Learning style
        var q5 = questions[4];
        options.AddRange(new[]
        {
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q5.Id,
                OptionText = "Video tutorials - I learn best by watching demonstrations",
                SkillLevel = "None",
                Category = "Learning Style"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q5.Id,
                OptionText = "Hands-on projects - I prefer learning by doing",
                SkillLevel = "None",
                Category = "Learning Style"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q5.Id,
                OptionText = "Reading and articles - I like detailed written explanations",
                SkillLevel = "None",
                Category = "Learning Style"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q5.Id,
                OptionText = "Interactive exercises - I enjoy quizzes and practice problems",
                SkillLevel = "None",
                Category = "Learning Style"
            }
        });

        await _context.AssessmentOptions.AddRangeAsync(options);
        await _context.SaveChangesAsync();
    }

    private async Task SeedInstructorsAsync()
    {
        if (!await _context.Users.AnyAsync(u => u.Role != null && u.Role.Name == "Instructor"))
        {
            var instructorRole = await _context.Roles.FirstAsync(r => r.Name == "Instructor");
            
            var instructors = new[]
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "john_dev",
                    Email = "john@learnhub.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    CreateAt = DateTime.UtcNow,
                    RoleId = instructorRole.Id
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "sarah_data",
                    Email = "sarah@learnhub.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    CreateAt = DateTime.UtcNow,
                    RoleId = instructorRole.Id
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "mike_design",
                    Email = "mike@learnhub.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    CreateAt = DateTime.UtcNow,
                    RoleId = instructorRole.Id
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "anna_business",
                    Email = "anna@learnhub.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    CreateAt = DateTime.UtcNow,
                    RoleId = instructorRole.Id
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "david_mobile",
                    Email = "david@learnhub.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    CreateAt = DateTime.UtcNow,
                    RoleId = instructorRole.Id
                }
            };

            await _context.Users.AddRangeAsync(instructors);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedComprehensiveCoursesAsync()
    {
        if (await _context.Courses.CountAsync() < 20) // Only add if we have fewer than 20 courses
        {
            var categories = await _context.Categories.ToListAsync();
            var instructors = await _context.Users.Include(u => u.Role).Where(u => u.Role!.Name == "Instructor").ToListAsync();

            var courses = new List<Course>();
            var courseData = GetComprehensiveCourseData();

            foreach (var data in courseData)
            {
                var category = categories.FirstOrDefault(c => c.Name == data.CategoryName);
                var instructor = instructors[Random.Shared.Next(instructors.Count)];
                
                if (category != null && instructor != null)
                {
                    var course = new Course
                    {
                        Id = Guid.NewGuid(),
                        Title = data.Title,
                        Description = data.Description,
                        Price = data.Price,
                        CategoryId = category.Id,
                        InstructorId = instructor.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 90)),
                        ImageUrl = data.ImageUrl,
                        Status = "Published"
                    };

                    courses.Add(course);
                }
            }

            await _context.Courses.AddRangeAsync(courses);
            await _context.SaveChangesAsync();

            // Add modules and lessons for courses
            await SeedModulesAndLessonsAsync(courses);
        }
    }

    private async Task SeedModulesAndLessonsAsync(List<Course> courses)
    {
        foreach (var course in courses)
        {
            var modules = new List<Module>();
            var moduleCount = Random.Shared.Next(3, 6); // 3-5 modules per course

            for (int i = 1; i <= moduleCount; i++)
            {
                var module = new Module
                {
                    Id = Guid.NewGuid(),
                    Title = $"Module {i}: {GetModuleTitle(course.Title, i)}",
                    Description = $"Learn the fundamentals of {GetModuleTitle(course.Title, i).ToLower()}",
                    OrderIndex = i,
                    CourseId = course.Id,
                    CreatedAt = DateTime.UtcNow
                };

                modules.Add(module);

                // Add lessons for each module
                var lessonCount = Random.Shared.Next(4, 8); // 4-7 lessons per module
                var lessons = new List<Lesson>();

                for (int j = 1; j <= lessonCount; j++)
                {
                    var lesson = new Lesson
                    {
                        Id = Guid.NewGuid(),
                        Title = $"Lesson {j}: {GetLessonTitle(module.Title, j)}",
                        Content = $"This lesson covers {GetLessonTitle(module.Title, j).ToLower()} in detail with practical examples and exercises.",
                        OrderIndex = j,
                        ModuleId = module.Id,
                        CreatedAt = DateTime.UtcNow,
                        VideoUrl = j <= 2 ? "https://example.com/preview-video" : null // First 2 lessons as preview
                    };

                    lessons.Add(lesson);
                }

                await _context.Lessons.AddRangeAsync(lessons);
            }

            await _context.Modules.AddRangeAsync(modules);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedLearningPathsAsync()
    {
        if (!await _context.LearningPaths.AnyAsync())
        {
            var courses = await _context.Courses.Include(c => c.Category).ToListAsync();
            var learningPaths = new List<LearningPath>();

            // Create structured learning paths
            var pathData = GetLearningPathData();

            foreach (var data in pathData)
            {
                var path = new LearningPath
                {
                    Id = Guid.NewGuid(),
                    Title = data.Title,
                    Description = data.Description,
                    Price = data.Price,
                    Status = "Published",
                    IsCustomPath = data.IsCustomPath,
                    CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 60))
                };

                learningPaths.Add(path);

                // Add courses to path
                var pathCourses = new List<PathCourse>();
                var relevantCourses = courses.Where(c => data.CategoryNames.Contains(c.Category.Name)).Take(data.CourseCount).ToList();

                for (int i = 0; i < relevantCourses.Count; i++)
                {
                    pathCourses.Add(new PathCourse
                    {
                        PathId = path.Id,
                        CourseId = relevantCourses[i].Id,
                        OrderIndex = i + 1
                    });
                }

                await _context.PathCourses.AddRangeAsync(pathCourses);
            }

            await _context.LearningPaths.AddRangeAsync(learningPaths);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedSampleUsersAndOrdersAsync()
    {
        var userRole = await _context.Roles.FirstAsync(r => r.Name == "User");
        
        // Create sample user if none exist
        if (!await _context.Users.AnyAsync(u => u.Role != null && u.Role.Name == "User"))
        {
            var sampleUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                CreateAt = DateTime.UtcNow,
                RoleId = userRole.Id
            };

            await _context.Users.AddAsync(sampleUser);
            await _context.SaveChangesAsync();

            // Create sample orders for the test user
            var courses = await _context.Courses.Take(3).ToListAsync();
            var learningPaths = await _context.LearningPaths.Take(2).ToListAsync();

            var orders = new List<Order>();

            // Create course orders
            foreach (var course in courses)
            {
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = sampleUser.Id,
                    TotalAmount = course.Price,
                    Status = Random.Shared.Next(0, 3) switch
                    {
                        0 => "Pending",
                        1 => "Completed",
                        _ => "Completed"
                    },
                    CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30)),
                    CompletedAt = Random.Shared.Next(0, 3) > 0 ? DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 20)) : null
                };

                orders.Add(order);

                // Create transaction for completed orders
                if (order.Status == "Completed")
                {
                    var transaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        Amount = order.TotalAmount,
                        Status = "Completed",
                        PaymentMethod = "Credit Card",
                        CreatedAt = order.CompletedAt ?? order.CreatedAt
                    };

                    await _context.Transactions.AddAsync(transaction);

                    // Create enrollment for completed course orders
                    var enrollment = new Enrollment
                    {
                        Id = Guid.NewGuid(),
                        UserId = sampleUser.Id,
                        CourseId = course.Id,
                        EnrolledAt = order.CompletedAt ?? order.CreatedAt,
                        Status = "Active"
                    };

                    await _context.Enrollments.AddAsync(enrollment);
                }
            }

            // Create learning path orders
            foreach (var path in learningPaths)
            {
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = sampleUser.Id,
                    TotalAmount = path.Price,
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 60)),
                    CompletedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 50))
                };

                orders.Add(order);

                // Create transaction
                var transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    Amount = order.TotalAmount,
                    Status = "Completed",
                    PaymentMethod = "PayPal",
                    CreatedAt = order.CompletedAt ?? order.CreatedAt
                };

                await _context.Transactions.AddAsync(transaction);

                // Create learning path enrollment
                var pathEnrollment = new UserLearningPathEnrollment
                {
                    Id = Guid.NewGuid(),
                    UserId = sampleUser.Id,
                    PathId = path.Id,
                    EnrolledAt = order.CompletedAt ?? order.CreatedAt,
                    Status = "Active"
                };

                await _context.UserLearningPathEnrollments.AddAsync(pathEnrollment);
            }

            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedAdminAsync()
    {
        if (!await _context.Users.AnyAsync(u => u.Username == "admin"))
        {
            var adminRole = await _context.Roles.FirstAsync(r => r.Name == "Admin");
            var admin = new User
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                Email = "admin@learnhub.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                CreateAt = DateTime.UtcNow,
                RoleId = adminRole.Id,
                IsActive = true
            };

            await _context.Users.AddAsync(admin);
            await _context.SaveChangesAsync();
        }
    }

    private List<(string Title, string Description, decimal Price, string CategoryName, string ImageUrl)> GetComprehensiveCourseData()
    {
        return new List<(string, string, decimal, string, string)>
        {
            // Web Development
            ("Complete Web Development Bootcamp", "Learn HTML, CSS, JavaScript, React, Node.js and build real-world projects from scratch", 49.99m, "Web Development", "https://images.unsplash.com/photo-1461749280684-dccba630e2f6?w=400&h=225&fit=crop"),
            ("Advanced JavaScript ES6+", "Master modern JavaScript features, async programming, and advanced concepts", 39.99m, "Web Development", "https://images.unsplash.com/photo-1579468118864-1b9ea3c0db4a?w=400&h=225&fit=crop"),
            ("React.js Complete Guide", "Build dynamic UIs with React, Redux, and modern development practices", 59.99m, "Web Development", "https://images.unsplash.com/photo-1555066931-4365d14bab8c?w=400&h=225&fit=crop"),
            ("Node.js Backend Development", "Create robust APIs and server applications with Node.js and Express", 44.99m, "Web Development", "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=400&h=225&fit=crop"),
            ("Vue.js Progressive Framework", "Learn Vue.js from basics to advanced concepts with real projects", 42.99m, "Web Development", "https://images.unsplash.com/photo-1593720213428-28a5b9e94613?w=400&h=225&fit=crop"),

            // Data Science
            ("Data Science with Python", "Master data analysis, visualization, and machine learning with Python libraries", 59.99m, "Data Science", "https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=400&h=225&fit=crop"),
            ("Machine Learning Fundamentals", "Introduction to ML algorithms, supervised and unsupervised learning", 69.99m, "Data Science", "https://images.unsplash.com/photo-1555949963-aa79dcee981c?w=400&h=225&fit=crop"),
            ("Deep Learning with TensorFlow", "Neural networks, deep learning architectures, and practical applications", 79.99m, "Data Science", "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400&h=225&fit=crop"),
            ("SQL for Data Analysis", "Master SQL queries, database design, and data manipulation techniques", 34.99m, "Data Science", "https://images.unsplash.com/photo-1544383835-bda2bc66a55d?w=400&h=225&fit=crop"),
            ("Statistics for Data Science", "Statistical foundations for data analysis and machine learning", 39.99m, "Data Science", "https://images.unsplash.com/photo-1543286386-713bdd548da4?w=400&h=225&fit=crop"),

            // Design
            ("UI/UX Design Fundamentals", "Learn user interface and user experience design principles and tools", 39.99m, "Design", "https://images.unsplash.com/photo-1561070791-2526d30994b5?w=400&h=225&fit=crop"),
            ("Adobe Creative Suite Mastery", "Master Photoshop, Illustrator, and InDesign for professional design", 54.99m, "Design", "https://images.unsplash.com/photo-1626785774625-0b1c2c4eab67?w=400&h=225&fit=crop"),
            ("Figma for UI Design", "Create stunning user interfaces and prototypes with Figma", 29.99m, "Design", "https://images.unsplash.com/photo-1596203671316-a11e6aa1d47b?w=400&h=225&fit=crop"),
            ("Design Systems & Style Guides", "Build consistent design systems and comprehensive style guides", 44.99m, "Design", "https://images.unsplash.com/photo-1572044162444-ad60f128bdea?w=400&h=225&fit=crop"),

            // Business
            ("Digital Marketing Mastery", "Complete guide to digital marketing, SEO, social media, and advertising", 44.99m, "Business", "https://images.unsplash.com/photo-1460925895917-afdab827c52f?w=400&h=225&fit=crop"),
            ("Project Management Professional", "Learn PMP methodologies, agile practices, and leadership skills", 49.99m, "Business", "https://images.unsplash.com/photo-1552664730-d307ca884978?w=400&h=225&fit=crop"),
            ("Entrepreneurship Bootcamp", "Start and scale your business with proven strategies and frameworks", 64.99m, "Business", "https://images.unsplash.com/photo-1553484771-371a605b060b?w=400&h=225&fit=crop"),
            ("Business Analytics & Intelligence", "Data-driven decision making and business intelligence tools", 39.99m, "Business", "https://images.unsplash.com/photo-1551434678-e076c223a692?w=400&h=225&fit=crop"),

            // Mobile Development
            ("iOS App Development with Swift", "Build native iOS applications from beginner to advanced level", 59.99m, "Mobile Development", "https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c?w=400&h=225&fit=crop"),
            ("Android Development with Kotlin", "Create modern Android apps using Kotlin and Android Studio", 54.99m, "Mobile Development", "https://images.unsplash.com/photo-1607252650355-f7fd0460ccdb?w=400&h=225&fit=crop"),
            ("React Native Cross-Platform", "Build mobile apps for iOS and Android with React Native", 49.99m, "Mobile Development", "https://images.unsplash.com/photo-1555774698-0b77e0d5fac6?w=400&h=225&fit=crop"),
            ("Flutter App Development", "Create beautiful native apps with Google's Flutter framework", 52.99m, "Mobile Development", "https://images.unsplash.com/photo-1551650975-87deedd944c3?w=400&h=225&fit=crop")
        };
    }

    private List<(string Title, string Description, decimal Price, bool IsCustomPath, int CourseCount, List<string> CategoryNames)> GetLearningPathData()
    {
        return new List<(string, string, decimal, bool, int, List<string>)>
        {
            ("Full Stack Web Developer", "Master both front-end and back-end development with modern technologies", 299.99m, false, 6, new List<string> { "Web Development" }),
            ("Data Science Professional", "From Python basics to advanced machine learning and AI applications", 399.99m, false, 5, new List<string> { "Data Science" }),
            ("UI/UX Design Master", "Complete design workflow from user research to high-fidelity prototypes", 249.99m, false, 4, new List<string> { "Design" }),
            ("Digital Marketing Expert", "Comprehensive digital marketing strategy and execution", 199.99m, false, 4, new List<string> { "Marketing", "Business" }),
            ("Mobile App Developer", "Build native and cross-platform mobile applications", 349.99m, false, 4, new List<string> { "Mobile Development" }),
            ("Business Analyst Professional", "Data-driven business analysis and strategic planning", 279.99m, false, 4, new List<string> { "Business", "Data Science" }),
            ("Full Stack Designer", "Combine design skills with front-end development", 319.99m, false, 5, new List<string> { "Design", "Web Development" }),
            ("DevOps Engineer", "Infrastructure, automation, and deployment strategies", 359.99m, false, 4, new List<string> { "DevOps", "Web Development" })
        };
    }

    private string GetModuleTitle(string courseTitle, int moduleIndex)
    {
        var moduleTitles = new Dictionary<int, string[]>
        {
            { 1, new[] { "Introduction & Setup", "Getting Started", "Fundamentals", "Basics Overview" } },
            { 2, new[] { "Core Concepts", "Essential Skills", "Building Blocks", "Foundation" } },
            { 3, new[] { "Intermediate Topics", "Advanced Concepts", "Practical Application", "Deep Dive" } },
            { 4, new[] { "Advanced Techniques", "Expert Level", "Mastery", "Professional Skills" } },
            { 5, new[] { "Final Projects", "Portfolio Building", "Real-world Applications", "Capstone" } }
        };

        var titles = moduleTitles.GetValueOrDefault(moduleIndex, new[] { $"Module {moduleIndex}", "Advanced Topics", "Specialized Content" });
        return titles[Random.Shared.Next(titles.Length)];
    }

    private string GetLessonTitle(string moduleTitle, int lessonIndex)
    {
        var baseTitles = new[] { "Introduction", "Overview", "Practical Examples", "Hands-on Practice", "Exercise", "Quiz", "Review", "Deep Dive" };
        return baseTitles[Random.Shared.Next(baseTitles.Length)];
    }

    private async Task SeedQuizzesAsync()
    {
        if (!await _context.Quizzes.AnyAsync())
        {
            var webCourse = await _context.Courses
                .Include(c => c.Modules)
                .ThenInclude(m => m.Lessons)
                .FirstOrDefaultAsync(c => c.Title.Contains("Complete Web Development Bootcamp"));

            if (webCourse != null && webCourse.Modules.Any())
            {
                var firstLesson = webCourse.Modules.First().Lessons.First();
                
                var quiz = new Quiz
                {
                    Id = Guid.NewGuid(),
                    Title = "Introduction to Web Development Quiz",
                    LessonId = firstLesson.Id
                };

                var q1 = new Question
                {
                    Id = Guid.NewGuid(),
                    QuizId = quiz.Id,
                    Text = "Which tag is used for the largest heading in HTML?"
                };

                q1.Options.Add(new Option { Id = Guid.NewGuid(), QuestionId = q1.Id, Text = "<h6>", IsCorrect = false });
                q1.Options.Add(new Option { Id = Guid.NewGuid(), QuestionId = q1.Id, Text = "<h1>", IsCorrect = true });
                q1.Options.Add(new Option { Id = Guid.NewGuid(), QuestionId = q1.Id, Text = "<head>", IsCorrect = false });

                var q2 = new Question
                {
                    Id = Guid.NewGuid(),
                    QuizId = quiz.Id,
                    Text = "What does CSS stand for?"
                };

                q2.Options.Add(new Option { Id = Guid.NewGuid(), QuestionId = q2.Id, Text = "Cascading Style Sheets", IsCorrect = true });
                q2.Options.Add(new Option { Id = Guid.NewGuid(), QuestionId = q2.Id, Text = "Computer Style Sheets", IsCorrect = false });
                q2.Options.Add(new Option { Id = Guid.NewGuid(), QuestionId = q2.Id, Text = "Creative Style Sheets", IsCorrect = false });

                quiz.Questions.Add(q1);
                quiz.Questions.Add(q2);

                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();
            }
        }
    }
}
