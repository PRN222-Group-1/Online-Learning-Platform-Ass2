using System;
using System.Collections.Generic;

namespace OnlineLearningPlatformAss2.Data.Entities;

public partial class LessonProgress
{
    public Guid ProgressId { get; set; }

    public Guid EnrollmentId { get; set; }

    public Guid LessonId { get; set; }

    public bool IsCompleted { get; set; }

    public int? LastWatchedPosition { get; set; }

    public string? AiSummary { get; set; }

    public string? AiSummaryStatus { get; set; }

    public string? Transcript { get; set; }

    public DateTime? LastAccessedAt { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;

    public virtual Lesson Lesson { get; set; } = null!;
}
