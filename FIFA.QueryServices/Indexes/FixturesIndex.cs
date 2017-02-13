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
            this.MaxIndexOutputsPerDocument = 500;

            Map =
                leagues =>
                from league in leagues
                from fixture in league.Fixtures
                let homePlayer = LoadDocument<Player>(fixture.HomePlayerId)
                let awayPlayer = LoadDocument<Player>(fixture.AwayPlayerId)
                let homeTeam = (from p in league.Participants
                                where p.PlayerId == fixture.HomePlayerId
                                select LoadDocument<Team>(p.TeamId)).First()
                let awayTeam = (from p in league.Participants
                                where p.PlayerId == fixture.AwayPlayerId
                                select LoadDocument<Team>(p.TeamId)).First()
                where fixture.Result == null
                select new FixtureSummary
                {
                    LeagueId = league.Id,
                    HomePlayerId = homePlayer.Id,
                    HomePlayerFace = homePlayer.Face,
                    HomePlayerName = homePlayer.Name,
                    HomeTeamBadge = homeTeam.Badge,
                    HomeTeamName = homeTeam.TeamName,
                    AwayPlayerId = awayPlayer.Id,
                    AwayPlayerFace = awayPlayer.Face,
                    AwayPlayerName = awayPlayer.Name,
                    AwayTeamBadge = awayTeam.Badge,
                    AwayTeamName = awayTeam.TeamName
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
                               AwayTeamBadge = f.AwayTeamBadge,
                               AwayTeamName = f.AwayTeamName,
                               HomeTeamBadge = f.HomeTeamBadge,
                               HomeTeamName = f.HomeTeamName
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
                               AwayTeamBadge = g.Key.AwayTeamBadge,
                               AwayTeamName = g.Key.AwayTeamName,
                               HomeTeamBadge = g.Key.HomeTeamBadge,
                               HomeTeamName = g.Key.HomeTeamName
                           };
        }
    }
}
