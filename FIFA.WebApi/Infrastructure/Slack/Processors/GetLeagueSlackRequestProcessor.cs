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
                response.AppendFormat(string.Format("\n{0} {1} {2} Played: *{3}* W: *{4}* D: *{5}* L: *{6}* GD: *{7}* Pts: *{8}*",
                    i++,
                    row.PlayerFace,
                    row.TeamBadge,
                    row.GamesPlayed,
                    row.GamesWon.ToString().PadLeft(3, ' '),
                    row.GamesDrawn.ToString().PadLeft(3, ' '),
                    row.GamesLost.ToString().PadLeft(3, ' '),
                    (row.GoalsFor - row.GoalsAgainst).ToString().PadLeft(3, ' '),
                    row.Points.ToString().PadLeft(3, ' ')));
            }

            SendResponse(request.response_url, response.ToString());
        }
    }
}