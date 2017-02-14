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

        public abstract ValidationResult ValidateRequest(SlackRequest request);

        protected void SendResponse(string url, string responseContent)
        {
            var response = new SlackSlashResponse(responseContent);

            SendResponse(url, response);
        }

        protected void SendResponse(string url, SlackSlashResponse response)
        {
            HttpClient client = new HttpClient();

            response.ResponseType = "in_channel";

            client.BaseAddress = new Uri(url);

            var result = client.PostAsJsonAsync(string.Empty, response).Result;

            var content = result.Content.ReadAsStringAsync().Result;

            content.ToString();
        }
    }
}