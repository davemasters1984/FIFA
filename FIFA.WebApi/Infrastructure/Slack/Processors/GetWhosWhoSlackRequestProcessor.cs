using FIFA.QueryServices.Interface;
using FIFA.WebApi.Models.Slack;
using System;
using System.Linq;
using System.Text;

namespace FIFA.WebApi.Infrastructure.Slack.Processors
{
    public class GetWhosWhoSlackRequestProcessor : SlackRequestProcessor
    {
        private ILeagueQueryService _queryService;
        private string _leagueName;

        public GetWhosWhoSlackRequestProcessor(ILeagueQueryService leagueQueryService)
        {
            _queryService = leagueQueryService;
        }

        public override string CommandText
        {
            get
            {
                return "whos-who";
            }
        }

        public override string ExampleRequest
        {
            get
            {
                return $"`{SlackSlashCommand} {CommandText} prem`";
            }
        }

        public override string Description
        {
            get
            {
                return "Returns a summary of statistics for the past week";
            }
        }

        public override ValidationResult ValidateRequest(SlackRequest request)
        {
            string[] commandWords = request.text.Split();

            if (commandWords.Length < 2)
                throw new Exception("`You must specify the league name`");

            _leagueName = commandWords[1];

            return ValidationResult.ValidResult(string.Format("`Fetching who's who`"));
        }

        protected override void ExecuteRequest(SlackRequest request)
        {
            var leagueId = _queryService.GetCurrentLeagueIdFromLeagueName(_leagueName);
            var league = _queryService.GetLeagueTable(leagueId);

            var response = new StringBuilder();

            response.AppendFormat("`Whos-who for league: {0}` \n", _leagueName);

            foreach (var row in league.OrderByDescending(r => r.TeamRating))
            {
                response.AppendFormat("\n{0} *{1}* is {2} *{3}* `[{4}]`",
                    row.PlayerFace,
                    row.PlayerName,
                    row.TeamBadge,
                    row.TeamName,
                    row.TeamRating);
            }

            SendResponse(request.response_url, response.ToString());
        }
    }
}