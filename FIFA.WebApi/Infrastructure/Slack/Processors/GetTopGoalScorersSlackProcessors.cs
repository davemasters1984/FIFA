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
        private ILeagueQueryService _leagueQueryService;
        private string _leagueName;

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

        public GetTopGoalScorersSlackProcessors(IStatisticQueryService queryService, 
            ILeagueQueryService leagueQueryService)
        {
            _queryService = queryService;
            _leagueQueryService = leagueQueryService;
        }

        public override ValidationResult ValidateRequest(SlackRequest request)
        {
            string[] commandWords = request.text.Split();

            if (commandWords.Length < 2)
                throw new Exception("`You must specify the league name`");

            _leagueName = commandWords[1];

            return ValidationResult.ValidResult("`Retreiving top scorers`");
        }

        protected override void ExecuteRequest(SlackRequest request)
        {
            var leagueId = _leagueQueryService.GetCurrentLeagueIdFromLeagueName(_leagueName);
            var topScorers = _queryService.GetCurrentTopGoalScorersForLeague(leagueId);

            var builder = new StringBuilder();

            builder.Append(string.Format("\n`Golden boot contenders for {0}:`", _leagueName));

            foreach (var player in topScorers.Players.Take(5))
                builder.AppendFormat("\n{0} :soccer: X {1}", player.Face, player.GoalsScored);

            var response = builder.ToString();

            SendResponse(request.response_url, response);
        }
    }
}