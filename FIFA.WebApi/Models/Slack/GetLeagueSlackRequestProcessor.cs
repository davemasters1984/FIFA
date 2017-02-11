using FIFA.Infrastructure.IoC;
using FIFA.QueryServices.Interface;
using Microsoft.Practices.Unity;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.WebApi.Models.Slack
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

        public GetLeagueSlackRequestProcessor()
        {
            _queryService = UnityHelper.Container.Resolve<ILeagueQueryService>();
        }

        protected override void Execute(SlackRequest request)
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
                    row.GamesWon,
                    row.GamesDrawn,
                    row.GamesLost,
                    row.GoalsFor - row.GoalsAgainst,
                    row.Points));
            }

            SendResponse(request.response_url, response.ToString());
        }
    }
}