using FIFA.QueryServices.Interface;
using FIFA.WebApi.Models.Slack;
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
                var positionChangeIcon = GetPositionChangeIcon(row.PositionChange);

                response.AppendFormat(string.Format("\n{0} {1} {2} Played: *{3}* W: *{4}* D: *{5}* L: *{6}* GD: *{7}* Pts: *{8}* {9}",
                    i++,
                    row.PlayerFace,
                    row.TeamBadge,
                    row.GamesPlayed,
                    row.GamesWon.ToString().PadLeft(3, ' '),
                    row.GamesDrawn.ToString().PadLeft(3, ' '),
                    row.GamesLost.ToString().PadLeft(3, ' '),
                    (row.GoalsFor - row.GoalsAgainst).ToString().PadLeft(3, ' '),
                    row.Points.ToString().PadLeft(3, ' '),
                    positionChangeIcon));
            }

            SendResponse(request.response_url, response.ToString());
        }

        private string GetPositionChangeIcon(int difference)
        {
            if (difference < 0)
                return ":arrow_up:";
            if (difference > 0)
                return ":arrow_down:";

            return string.Empty;
        }

        public override ValidationResult ValidateRequest(SlackRequest request)
        {
            return ValidationResult.ValidResult("`Retreiving league table`");
        }
    }
}