using FIFA.WebApi.Models.Slack;

namespace FIFA.WebApi.Infrastructure.Slack
{
    public interface ISlackRequestProcessor
    {
        string CommandText { get; }

        string ExampleRequest { get; }

        string Description { get; }

        void Execute(SlackRequest request);

        ValidationResult ValidateRequest(SlackRequest request);
    }
}