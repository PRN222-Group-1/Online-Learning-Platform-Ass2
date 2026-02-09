using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatformAss2.Service.Services.Interfaces
{
    public interface ITranscriptService
    {
        Task<string> GenerateTranscriptFromVideoAsync(string videoUrl, bool summarize = true);
    }
}
