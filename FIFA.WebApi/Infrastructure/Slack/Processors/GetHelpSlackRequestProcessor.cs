using FIFA.WebApi.Models.Slack;
using System.Collections.Generic;
using System.Text;

namespace FIFA.WebApi.Infrastructure.Slack.Processors
{
    public class GetHelpSlackRequestProcessor : SlackRequestProcessor
    {
        private IEnumerable<ISlackRequestProcessor> _allProcessors;

        public GetHelpSlackRequestProcessor(ISlackRequestProcessor[] allProcessors)
        {
            _allProcessors = allProcessors;
        }

        public override string CommandText
        {
            get
            {
                return "help";
            }
        }

        public override string Description { get; }

        public override string ExampleRequest { get; }

        public override ValidationResult ValidateRequest(SlackRequest request)
        {
            return ValidationResult.ValidResult("`Getting the help you need....`");
        }

        protected override void ExecuteRequest(SlackRequest request)
        {
            StringBuilder response = new StringBuilder();

            foreach (var processor in _allProcessors)
            {
                response.Append($"\n{processor.ExampleRequest}");
                response.Append($"\n{processor.Description}");
                response.Append($"\n\n");
            }

            var responseString = response.ToString();

            SendResponse(request.response_url, responseString);
        }
    }
}