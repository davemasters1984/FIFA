using FIFA.QueryServices.Interface;
using FIFA.WebApi.Models.Slack;
using System;
using System.Linq;
using System.Text;

namespace FIFA.WebApi.Infrastructure.Slack
{
    public class GetLeagueSlackRequestProcessor : SlackRequestProcessor
    {
        private ILeagueQueryService _queryService;
        private string _leagueName;

        public override string CommandText
        {
            get
            {
                return "table";
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
                return "Returns the league table";
            }
        }

        public GetLeagueSlackRequestProcessor(ILeagueQueryService queryService)
        {
            _queryService = queryService;
        }

        private const string _topLeagueHeader = ". . : : : / / / :star: :soccer: :trophy-prem: *{0} TABLE* :trophy-prem: :soccer: :star: \\ \\ \\ : : : . .\n";
        private const string _bottomLeagueHeader = ". . : : : / / :balloon: :soccer: :trophy-champ: *{0} TABLE* :trophy-champ: :soccer: :balloon: \\ \\ : : : . .\n";

        protected override void ExecuteRequest(SlackRequest request)
        {
            var leagueId = _queryService.GetCurrentLeagueIdFromLeagueName(_leagueName);
            var leagueTable = _queryService.GetLeagueTableWithHeader(leagueId);

            var response = new StringBuilder();
            int currentPosition = 0;
            int relegationPosition = leagueTable.Rows.Count();
            int relegationPlayOffPosition = leagueTable.Rows.Count() - 2;
            int promotionPlayOffPosition = 3;
            var relegationIcon = ":skull:";
            var relegationPlayOffIcon = ":scream:";
            var promotionIcon = ":sunglasses:";
            var promotionPlayOffIcon = ":slightly_smiling_face:";

            if (leagueTable.IsTopLeague)
                response.AppendFormat(_topLeagueHeader, _leagueName.ToUpper());
            if (leagueTable.IsBottomLeague)
                response.AppendFormat(_bottomLeagueHeader, _leagueName.ToUpper());

            foreach (var row in leagueTable.Rows)
            {
                currentPosition++;
                var positionString = GetFormattedNumberString(currentPosition);
                var positionChangeIcon = GetPositionChangeIcon(row.PositionChange);
                var positionChangeNumber = GetPositionChangeNumber(row.PositionChange);

                var gamesPlayedString = GetFormattedNumberString(row.GamesPlayed);
                var gamesWonString = GetFormattedNumberString(row.GamesWon);
                var gamesDrawnString = GetFormattedNumberString(row.GamesDrawn);
                var gamesLostString = GetFormattedNumberString(row.GamesLost);
                var goalDifferenceString = GetGoalDifferenceNumberString(row.GoalsFor - row.GoalsAgainst);
                var pointsString = GetFormattedNumberString(row.Points);


                var leaguePositionIcon = string.Empty;

                if (leagueTable.IsTopLeague)
                {
                    leaguePositionIcon = (currentPosition >= relegationPosition)
                        ? relegationIcon
                        : (currentPosition >= relegationPlayOffPosition)
                            ? relegationPlayOffIcon
                            : "     ";

                    if (currentPosition == relegationPosition || currentPosition == relegationPlayOffPosition)
                        response.Append("\n-----------------------------------------------------------------------------------");
                }

                if (leagueTable.IsBottomLeague)
                {
                    leaguePositionIcon = (currentPosition == 1)
                        ? promotionIcon
                        : (currentPosition <= promotionPlayOffPosition)
                            ? promotionPlayOffIcon
                            : "     ";


                    if (currentPosition == 2 || currentPosition == (promotionPlayOffPosition +1))
                        response.Append("\n-----------------------------------------------------------------------------------");
                }

                response.AppendFormat(string.Format("\n{0}{1} {2} Played: *{3}*   W: *{4}*   D: *{5}*   L: *{6}*   GD: {7}   Pts: *{8}* {11} {9} {10}",
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
                    positionChangeNumber,
                    leaguePositionIcon));
            }

            var responseString = response.ToString();

            SendResponse(request.response_url, responseString);
        }

        private string GetGoalDifferenceNumberString(int goalDifference)
        {
            if (goalDifference < -9)
                return string.Format("*{0}*", goalDifference);
            if (goalDifference > 9)
                return string.Format(" *{0}*", goalDifference);
            if (goalDifference < 0)
                return string.Format("*{0}* ", goalDifference);

            return string.Format(" *{0}*  ", goalDifference);
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
            string[] commandWords = request.text.Split();

            if (commandWords.Length < 2)
                return ValidationResult.InvalidResult("`You must specify the league name`");

            _leagueName = commandWords[1];

            return ValidationResult.ValidResult(string.Format("`Retreiving {0} table`", _leagueName));
        }
    }
}