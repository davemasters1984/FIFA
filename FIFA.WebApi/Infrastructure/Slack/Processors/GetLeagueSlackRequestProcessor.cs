using FIFA.QueryServices.Interface;
using FIFA.WebApi.Models.Slack;
using System;
using System.Text;

namespace FIFA.WebApi.Infrastructure.Slack
{
    public class GetLeagueSlackRequestProcessor : SlackRequestProcessor
    {
        private ILeagueQueryService _queryService;

        public override string CommandText
        {
            get
            {
                return "league";
            }
        }

        public GetLeagueSlackRequestProcessor(ILeagueQueryService queryService)
        {
            _queryService = queryService;
        }

        public override void Execute(SlackRequest request)
        {
            var leagueTable = _queryService.GetCurrentLeagueTable();

            var response = new StringBuilder();
            int i = 1;

            foreach (var row in leagueTable)
            {
                var positionString = GetFormattedNumberString(i++);
                var positionChangeIcon = GetPositionChangeIcon(row.PositionChange);
                var positionChangeNumber = GetPositionChangeNumber(row.PositionChange);

                var gamesPlayedString = GetFormattedNumberString(row.GamesPlayed);
                var gamesWonString = GetFormattedNumberString(row.GamesWon);
                var gamesDrawnString = GetFormattedNumberString(row.GamesDrawn);
                var gamesLostString = GetFormattedNumberString(row.GamesLost);
                var goalDifferenceString = GetFormattedNumberString(row.GoalsFor - row.GoalsAgainst);
                var pointsString = GetFormattedNumberString(row.Points);

                response.AppendFormat(string.Format("\n{0}{1} {2} Played: *{3}*   W: *{4}*   D: *{5}*   L: *{6}*   GD: *{7}*   Pts: *{8}* {9} {10}",
                    positionString,
                    row.PlayerFace,
                    row.TeamBadge,
                    gamesPlayedString,
                    gamesWonString,
                    gamesDrawnString,
                    gamesLostString,
                    goalDifferenceString,
                    pointsString,
                    positionChangeIcon,
                    positionChangeNumber));
            }

            var responseString = response.ToString();

            SendResponse(request.response_url, responseString);
        }

        private string GetFormattedNumberString(int number)
        {
            if (number < 0)
                return string.Format("{0}", number);

            if (number < 10)
                return string.Format("{0}  ", number);

            return string.Format("{0}", number);
        }

        private string GetPositionChangeIcon(int difference)
        {
            if (difference < 0)
                return ":arrow-up-green:";
            if (difference > 0)
                return ":arrow-down-red:";

            return string.Empty;
        }

        private string GetPositionChangeNumber(int difference)
        {
            if (difference < 0)
                return string.Format("[+{0}]", Math.Abs(difference));
            if (difference > 0)
                return string.Format("[-{0}]", Math.Abs(difference));

            return string.Empty;
        }

        public override ValidationResult ValidateRequest(SlackRequest request)
        {
            return ValidationResult.ValidResult("`Retreiving league table`");
        }
    }
}