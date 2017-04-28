using FIFA.WebApi.Infrastructure.Slack.Processors;
using FIFA.WebApi.Models.Slack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FIFA.WebApi.Infrastructure.Slack
{
    public class SlackRequestService : ISlackRequestService
    {
        private IEnumerable<ISlackRequestProcessor> _processors;
        private GetHelpSlackRequestProcessor _helpProcessor;

        public SlackRequestService(ISlackRequestProcessor[] processors)
        {
            _processors = processors;
            _helpProcessor = new GetHelpSlackRequestProcessor(processors);
        }

        public ValidationResult ValidateRequest(SlackRequest request)
        {
            if (request.text.ToLower() == _helpProcessor.CommandText.ToLower())
                return _helpProcessor.ValidateRequest(request);

            var commandText = GetCommandTextFromRequest(request.text);

            var command = _processors.FirstOrDefault(c => c.CommandText == commandText);

            if (command == null)
                return ValidationResult.InvalidResult(string.Format("'{0}' not recognised"));

            return command.ValidateRequest(request);
        }

        public async Task ExecuteRequestAsync(SlackRequest request)
        {
            if (request.text.ToLower() == _helpProcessor.CommandText.ToLower())
            {
                await Task.Factory.StartNew(() => _helpProcessor.Execute(request));
                return;
            }

            var commandText = GetCommandTextFromRequest(request.text);

            var command = _processors.FirstOrDefault(c => c.CommandText == commandText);

            if (command == null)
                throw new Exception("Command not supported");

            await Task.Factory.StartNew(() => command.Execute(request));
        }

        private static string GetCommandTextFromRequest(string commandText)
        {
            string[] texts = commandText.Split(' ');

            if (texts.Length > 0)
                return texts[0].ToLower();

            return string.Empty;
        }
    }
}