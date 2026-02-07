-- Add missing columns to tables

-- 1. Courses - Add IsFeatured, Level, Language, RejectionReason
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'is_featured' AND object_id = OBJECT_ID('Courses'))
    ALTER TABLE Courses ADD is_featured BIT NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'level' AND object_id = OBJECT_ID('Courses'))
    ALTER TABLE Courses ADD level NVARCHAR(50) NOT NULL DEFAULT 'All Levels';

IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'language' AND object_id = OBJECT_ID('Courses'))
    ALTER TABLE Courses ADD language NVARCHAR(50) NOT NULL DEFAULT 'English';

IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'rejection_reason' AND object_id = OBJECT_ID('Courses'))
    ALTER TABLE Courses ADD rejection_reason NVARCHAR(MAX) NULL;

-- 2. Modules - Add description
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'description' AND object_id = OBJECT_ID('Modules'))
    ALTER TABLE Modules ADD description NVARCHAR(MAX) NULL;

-- 3. Lessons - Add content (rename content_url to just have separate content field)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'content' AND object_id = OBJECT_ID('Lessons'))
    ALTER TABLE Lessons ADD content NVARCHAR(MAX) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'video_url' AND object_id = OBJECT_ID('Lessons'))
    ALTER TABLE Lessons ADD video_url NVARCHAR(MAX) NULL;

-- 4. LessonProgress - Add last_accessed_at
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'last_accessed_at' AND object_id = OBJECT_ID('Lesson_Progress'))
    ALTER TABLE Lesson_Progress ADD last_accessed_at DATETIME2 NULL;

-- 5. Enrollment - Add completed_at
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'completed_at' AND object_id = OBJECT_ID('Enrollments'))
    ALTER TABLE Enrollments ADD completed_at DATETIME2 NULL;

-- 6. Categories - Add description  
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'description' AND object_id = OBJECT_ID('Categories'))
    ALTER TABLE Categories ADD description NVARCHAR(MAX) NULL;

-- 7. Modules - Add created_at
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'created_at' AND object_id = OBJECT_ID('Modules'))
    ALTER TABLE Modules ADD created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE();

-- 8. Lessons - Add created_at
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'created_at' AND object_id = OBJECT_ID('Lessons'))
    ALTER TABLE Lessons ADD created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE();

PRINT 'All columns added successfully';
