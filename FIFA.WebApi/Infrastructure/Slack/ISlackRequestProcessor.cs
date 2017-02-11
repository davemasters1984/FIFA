using FIFA.WebApi.Models.Slack;

namespace FIFA.WebApi.Infrastructure.Slack
{
    public interface ISlackRequestProcessor
    {
        string CommandText { get; }

        void Execute(SlackRequest request);
    }
}