-- Add more missing columns

-- 1. UserAssessment - Add result_summary
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'result_summary' AND object_id = OBJECT_ID('User_Assessments'))
    ALTER TABLE User_Assessments ADD result_summary NVARCHAR(MAX) NULL;

-- 2. AssessmentOption - Add category
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'category' AND object_id = OBJECT_ID('Assessment_Options'))
    ALTER TABLE Assessment_Options ADD category NVARCHAR(100) NULL;

-- 3. Users - Add is_active column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'is_active' AND object_id = OBJECT_ID('users'))
    ALTER TABLE users ADD is_active BIT NOT NULL DEFAULT 1;

PRINT 'Additional columns added successfully';
