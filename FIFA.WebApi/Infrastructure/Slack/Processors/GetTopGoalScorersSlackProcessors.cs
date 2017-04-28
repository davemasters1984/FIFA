using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FIFA.WebApi.Models.Slack;
using FIFA.QueryServices.Interface;
using System.Text;

namespace FIFA.WebApi.Infrastructure.Slack.Processors
{
    public class GetTopGoalScorersSlackProcessors : SlackRequestProcessor
    {
        private IStatisticQueryService _queryService;

        public override string CommandText
        {
            get
            {
                return "golden-boot";
            }
        }

        public override string ExampleRequest
        {
            get
            {
                return $"`{SlackSlashCommand} {CommandText}`";
            }
        }

        public override string Description
        {
            get
            {
                return "Returns the top 5 goal scorers in the league ";
            }
        }

        public GetTopGoalScorersSlackProcessors(IStatisticQueryService queryService)
        {
            _queryService = queryService;
        }

        public override ValidationResult ValidateRequest(SlackRequest request)
        {
            return ValidationResult.ValidResult("`Retreiving top scorers`");
        }

        protected override void ExecuteRequest(SlackRequest request)
        {
            var topScorers = _queryService.GetCurrentTopGoalScorersForLeague("leagues/417");

            var builder = new StringBuilder();

            builder.Append("\n`Golden boot contenders:`");

            foreach (var player in topScorers.Players.Take(5))
                builder.AppendFormat("\n{0} :soccer: X {1}", player.Face, player.GoalsScored);

            var response = builder.ToString();

            SendResponse(request.response_url, response);
        }
    }
}