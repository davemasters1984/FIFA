using FIFA.WebApi.Models.Slack;
using System.Threading.Tasks;

namespace FIFA.WebApi.Infrastructure.Slack
{
    public interface ISlackRequestService
    {
        ValidationResult ValidateRequest(SlackRequest request);

        Task ExecuteRequestAsync(SlackRequest request);
    }

    public class ValidationResult
    {
        public string Message { get; set; }

        public bool IsValid { get; set; }

        public static ValidationResult ValidResult(string message)
        {
            return new ValidationResult { IsValid = true, Message = message };
        }

        public static ValidationResult InvalidResult(string message)
        {
            return new ValidationResult { Message = message };
        }
    }
}