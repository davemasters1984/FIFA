using FIFA.Model;
using FIFA.QueryServices.Interface.Models;
using FIFA.QueryServices.Models;
using Raven.Client.Indexes;
using System.Linq;

namespace FIFA.QueryServices.Indexes
{
    public class ResultsIndex : AbstractIndexCreationTask<League, ResultSummary>
    {
        public ResultsIndex()
        {
            this.MaxIndexOutputsPerDocument = 500;

            Map =
                leagues =>
                from league in leagues
                from fixture in league.Fixtures
                let homePlayer = LoadDocument<Player>(fixture.HomePlayerId)
                let awayPlayer = LoadDocument<Player>(fixture.AwayPlayerId)
                where fixture.Result != null
                select new ResultSummary
                {
                    LeagueId = league.Id,
                    HomePlayerId = homePlayer.Id,
                    HomePlayerFace = homePlayer.Face,
                    HomePlayerName = homePlayer.Name,
                    HomePlayerGoals = fixture.Result.HomePlayerGoals,
                    AwayPlayerId = awayPlayer.Id,
                    AwayPlayerFace = awayPlayer.Face,
                    AwayPlayerName = awayPlayer.Name,
                    AwayPlayerGoals = fixture.Result.AwayPlayerGoals,
                };

            Reduce =
                results => from r in results
                           group r by new
                           {
                               LeagueId = r.LeagueId,
                               HomePlayerId = r.HomePlayerId,
                               HomePlayerFace = r.HomePlayerFace,
                               HomePlayerName = r.HomePlayerName,
                               HomePlayerGoals = r.HomePlayerGoals,
                               AwayPlayerId = r.AwayPlayerId,
                               AwayPlayerFace = r.AwayPlayerFace,
                               AwayPlayerName = r.AwayPlayerName,
                               AwayPlayerGoals = r.AwayPlayerGoals,
                           } 
                           into g
                           select new ResultSummary
                           {
                               LeagueId = g.Key.LeagueId,
                               HomePlayerId = g.Key.HomePlayerId,
                               HomePlayerFace = g.Key.HomePlayerFace,
                               HomePlayerName = g.Key.HomePlayerName,
                               HomePlayerGoals = g.Key.HomePlayerGoals,
                               AwayPlayerId = g.Key.AwayPlayerId,
                               AwayPlayerFace = g.Key.AwayPlayerFace,
                               AwayPlayerName = g.Key.AwayPlayerName,
                               AwayPlayerGoals = g.Key.AwayPlayerGoals,
                           };
        }
    }
}