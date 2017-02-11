using FIFA.Model;
using FIFA.QueryServices.Interface.Models;
using Raven.Client.Indexes;
using System.Linq;

namespace FIFA.QueryServices.Indexes
{
    public class FixturesIndex : AbstractIndexCreationTask<League, FixtureSummary>
    {
        public FixturesIndex()
        {
            Map =
                leagues =>
                from league in leagues
                from fixture in league.Fixtures
                let homePlayer = LoadDocument<Player>(fixture.HomePlayerId)
                let awayPlayer = LoadDocument<Player>(fixture.AwayPlayerId)
                where fixture.Result == null
                select new ResultSummary
                {
                    LeagueId = league.Id,
                    HomePlayerId = homePlayer.Id,
                    HomePlayerFace = homePlayer.Face,
                    HomePlayerName = homePlayer.Name,
                    AwayPlayerId = awayPlayer.Id,
                    AwayPlayerFace = awayPlayer.Face,
                    AwayPlayerName = awayPlayer.Name,
                };

            Reduce =
                fixtures => from f in fixtures
                           group f by new
                           {
                               LeagueId = f.LeagueId,
                               HomePlayerId = f.HomePlayerId,
                               HomePlayerFace = f.HomePlayerFace,
                               HomePlayerName = f.HomePlayerName,
                               AwayPlayerId = f.AwayPlayerId,
                               AwayPlayerFace = f.AwayPlayerFace,
                               AwayPlayerName = f.AwayPlayerName,
                           }
                           into g
                           select new FixtureSummary
                           {
                               LeagueId = g.Key.LeagueId,
                               HomePlayerId = g.Key.HomePlayerId,
                               HomePlayerFace = g.Key.HomePlayerFace,
                               HomePlayerName = g.Key.HomePlayerName,
                               AwayPlayerId = g.Key.AwayPlayerId,
                               AwayPlayerFace = g.Key.AwayPlayerFace,
                               AwayPlayerName = g.Key.AwayPlayerName,
                           };
        }
    }
}
