using FIFA.WebApi.Infrastructure.Slack;
using FIFA.WebApi.Models.Slack;
using Microsoft.AspNet.WebHooks;
using System;
using System.Net.Http;

namespace FIFA.WebApi.Infrastructure.Slack
{
    public abstract class SlackRequestProcessor : ISlackRequestProcessor
    {
        public abstract string CommandText { get; }

        public abstract void Execute(SlackRequest request);

        protected void SendResponse(string url, string responseContent)
        {
            HttpClient client = new HttpClient();
            var response = new SlackSlashResponse(responseContent);

            client.BaseAddress = new Uri(url);

            var result = client.PostAsJsonAsync(string.Empty, response).Result;

            var content = result.Content.ReadAsStringAsync().Result;

            content.ToString();
        }
    }
}