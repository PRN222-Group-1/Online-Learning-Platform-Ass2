namespace OnlineLearningPlatformAss2.Service.DTOs.Quiz;

public class QuizViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public List<QuestionViewModel> Questions { get; set; } = new();
}

public class QuestionViewModel
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
    public List<OptionViewModel> Options { get; set; } = new();
}

public class OptionViewModel
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
}

public class QuizSubmissionDto
{
    public Guid QuizId { get; set; }
    public List<AnswerSubmissionDto> Answers { get; set; } = new();
}

public class AnswerSubmissionDto
{
    public Guid QuestionId { get; set; }
    public Guid SelectedOptionId { get; set; }
}

public class QuizResultDto
{
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public bool Passed { get; set; }
}
