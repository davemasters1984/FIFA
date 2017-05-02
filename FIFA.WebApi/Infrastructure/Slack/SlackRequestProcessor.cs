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

        public abstract string ExampleRequest { get; }

        public abstract string Description { get; }

        public string SlackSlashCommand { get { return "/betafifa"; } }

        protected abstract void ExecuteRequest(SlackRequest request);

        public abstract ValidationResult ValidateRequest(SlackRequest request);

        public void Execute(SlackRequest request)
        {
            try
            {
                ExecuteRequest(request);
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("`Command: '{0}' failed. Error: {1}`", request.text, ex.Message);
                SendResponse(request.response_url, errorMessage);
                throw;
            }
        }

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

        protected string GetFormattedNumberString(int number)
        {
            if (number < 0)
                return string.Format("{0}", number);

            if (number < 10)
                return string.Format("{0}  ", number);

            return string.Format("{0}", number);
        }
    }
}