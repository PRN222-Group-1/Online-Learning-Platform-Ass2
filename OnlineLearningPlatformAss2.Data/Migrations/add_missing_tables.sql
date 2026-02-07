-- Add missing tables to the database
-- These tables exist in code but not in the database

-- 1. Wishlist table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Wishlists')
BEGIN
    CREATE TABLE Wishlists (
        wishlist_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        user_id UNIQUEIDENTIFIER NOT NULL,
        course_id UNIQUEIDENTIFIER NOT NULL,
        added_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Wishlists_Users FOREIGN KEY (user_id) REFERENCES users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Wishlists_Courses FOREIGN KEY (course_id) REFERENCES Courses(course_id) ON DELETE CASCADE
    );
    CREATE INDEX IX_Wishlists_user_id ON Wishlists(user_id);
    CREATE INDEX IX_Wishlists_course_id ON Wishlists(course_id);
END;

-- 2. CourseReviews table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Course_Reviews')
BEGIN
    CREATE TABLE Course_Reviews (
        review_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        course_id UNIQUEIDENTIFIER NOT NULL,
        user_id UNIQUEIDENTIFIER NOT NULL,
        rating INT NOT NULL CHECK (rating >= 1 AND rating <= 5),
        comment NVARCHAR(MAX),
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_CourseReviews_Courses FOREIGN KEY (course_id) REFERENCES Courses(course_id) ON DELETE CASCADE,
        CONSTRAINT FK_CourseReviews_Users FOREIGN KEY (user_id) REFERENCES users(Id) ON DELETE NO ACTION
    );
    CREATE INDEX IX_Course_Reviews_course_id ON Course_Reviews(course_id);
    CREATE INDEX IX_Course_Reviews_user_id ON Course_Reviews(user_id);
END;

-- 3. LessonComments table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Lesson_Comments')
BEGIN
    CREATE TABLE Lesson_Comments (
        comment_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        lesson_id UNIQUEIDENTIFIER NOT NULL,
        user_id UNIQUEIDENTIFIER NOT NULL,
        content NVARCHAR(MAX) NOT NULL,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        parent_id UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_LessonComments_Lessons FOREIGN KEY (lesson_id) REFERENCES Lessons(lesson_id) ON DELETE CASCADE,
        CONSTRAINT FK_LessonComments_Users FOREIGN KEY (user_id) REFERENCES users(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_LessonComments_Parent FOREIGN KEY (parent_id) REFERENCES Lesson_Comments(comment_id) ON DELETE NO ACTION
    );
    CREATE INDEX IX_Lesson_Comments_lesson_id ON Lesson_Comments(lesson_id);
    CREATE INDEX IX_Lesson_Comments_user_id ON Lesson_Comments(user_id);
    CREATE INDEX IX_Lesson_Comments_parent_id ON Lesson_Comments(parent_id);
END;

-- 4. Notifications table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE Notifications (
        notification_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        user_id UNIQUEIDENTIFIER NOT NULL,
        message NVARCHAR(MAX) NOT NULL,
        type NVARCHAR(50) NOT NULL DEFAULT 'General',
        is_read BIT NOT NULL DEFAULT 0,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Notifications_Users FOREIGN KEY (user_id) REFERENCES users(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_Notifications_user_id ON Notifications(user_id);
END;

-- 5. ChatMessages table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Chat_Messages')
BEGIN
    CREATE TABLE Chat_Messages (
        message_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        sender_id UNIQUEIDENTIFIER NOT NULL,
        receiver_id UNIQUEIDENTIFIER NULL,
        content NVARCHAR(MAX) NOT NULL,
        is_from_admin BIT NOT NULL DEFAULT 0,
        sent_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_ChatMessages_Sender FOREIGN KEY (sender_id) REFERENCES users(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_ChatMessages_Receiver FOREIGN KEY (receiver_id) REFERENCES users(Id) ON DELETE NO ACTION
    );
    CREATE INDEX IX_Chat_Messages_sender_id ON Chat_Messages(sender_id);
    CREATE INDEX IX_Chat_Messages_receiver_id ON Chat_Messages(receiver_id);
END;

-- 6. Add CreatedAt column to users table if it doesn't exist (rename from CreateAt)
IF EXISTS (SELECT * FROM sys.columns WHERE name = 'CreateAt' AND object_id = OBJECT_ID('users'))
BEGIN
    EXEC sp_rename 'users.CreateAt', 'created_at', 'COLUMN';
END;

-- 7. Add PasswordHash column if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'PasswordHash' AND object_id = OBJECT_ID('users'))
BEGIN
    ALTER TABLE users ADD PasswordHash NVARCHAR(500) NOT NULL DEFAULT '';
END;
