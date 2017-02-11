using Microsoft.AspNet.WebHooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.WebApi.Models.Slack
{
    public abstract class SlackRequestProcessor
    {
        private SlackRequest Request { get; set; }

        public static List<SlackRequestProcessor> Commands
        {
            get
            {
                return new List<SlackRequestProcessor>
                {
                    new GetLeagueSlackRequestProcessor(),
                    new PostResultSlackRequestProcessor()
                };
            }
        }

        public abstract string CommandText { get; }

        protected abstract void Execute(SlackRequest request);

        protected void SendResponse(string url, string responseContent)
        {
            HttpClient client = new HttpClient();
            var response = new SlackSlashResponse(responseContent);

            client.BaseAddress = new Uri(url);

            var result = client.PostAsJsonAsync(string.Empty, response).Result;

            var content = result.Content.ReadAsStringAsync().Result;

            content.ToString();
        }

        public async static Task ExecuteRequestAsync(SlackRequest request)
        {
            var commandText = GetCommandTextFromRequest(request.text);

            var command = Commands.FirstOrDefault(c => c.CommandText == commandText);

            if (command == null)
                throw new Exception("Command not supported");

            //var task = new Task(() => command.Execute(request));

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