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

        public SlackRequestService(ISlackRequestProcessor[] processors)
        {
            _processors = processors;
        }

        public async Task ExecuteRequestAsync(SlackRequest request)
        {
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