using System;
using System.Collections.Generic;
using System.Linq;

namespace FIFA.WebApi.Models.Slack
{
    public abstract class SlackCommand
    {
        private SlackRequest Request { get; set; }

        public static List<SlackCommand> Commands
        {
            get
            {
                return new List<SlackCommand>
                {
                    new GetLeagueSlackCommand(),
                    new PostResultSlackCommand()
                };
            }
        }

        public abstract string CommandText { get; }

        public abstract string Execute(SlackRequest request);

        public static string ExecuteRequest(SlackRequest request)
        {
            var commandText = GetCommandTextFromRequest(request.text);

            var command = Commands.FirstOrDefault(c => c.CommandText == commandText);

            if (command == null)
                throw new Exception("Command not supported");

            return command.Execute(request);
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