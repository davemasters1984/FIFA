using FIFA.Infrastructure.IoC;
using FIFA.QueryServices.Interface;
using Microsoft.Practices.Unity;
using System.Text;

namespace FIFA.WebApi.Models.Slack
{
    public class GetLeagueSlackCommand : SlackCommand
    {
        private ILeagueQueryService _queryService;

        public override string CommandText
        {
            get
            {
                return "league";
            }
        }

        public GetLeagueSlackCommand()
        {
            _queryService = UnityHelper.Container.Resolve<ILeagueQueryService>();
        }

        public override string Execute(SlackRequest request)
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

            return response.ToString();
        }
    }
}