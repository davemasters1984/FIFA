using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FIFA.WebApi.Models.Slack;
using Microsoft.AspNet.WebHooks;

namespace FIFA.WebApi.Infrastructure.Slack.Processors
{
    public class GetPlayerComparisonHistorySlackRequestProcessor : SlackRequestProcessor
    {
        private string _playerOneFace;
        private string _playerTwoFace;
        private string _orginalRequestUrl;
        public override string CommandText
        {
            get
            {
                return "compare";
            }
        }

        public override void Execute(SlackRequest request)
        {
            var response = new SlackSlashResponse("Comparison Chart!");

            response.Attachments.Add(new SlackAttachment("Chart", "Bla")
            {
                ImageLink = new Uri(_orginalRequestUrl + "/api/leagues/417/players/79")
            });

            SendResponse(request.response_url, response);
        }

        public override ValidationResult ValidateRequest(SlackRequest request)
        {
            _orginalRequestUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

            return ValidationResult.ValidResult("Getting comparison chart");
        }
    }
}