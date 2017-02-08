using FIFA.Model;
using FIFA.QueryServices.Models;
using Raven.Client.Indexes;
using System.Linq;

namespace FIFA.QueryServices.Indexes
{
    public class LeagueTableIndex : AbstractIndexCreationTask<League, LeagueTableRow>
    {
        public LeagueTableIndex()
        {
            Map =
                leagues =>
                from league in leagues
                from p in league.Participants
                let team = LoadDocument<Team>(p.TeamId)
                let player = LoadDocument<Player>(p.PlayerId)
                select new LeagueTableRow
                {
                    LeagueId = league.Id,
                    PlayerName = player.Name,
                    TeamName = team.TeamName,
                    GoalsAgainst = p.GoalsAgainst,
                    GoalsFor = p.GoalsFor,
                    Position = p.Position,
                    Points = p.Points,
                    GamesPlayed = p.GamesPlayed,
                    TeamRating = team.OverallRating
                };

            Reduce =
                results => from r in results
                           group r by new
                           {
                               LeagueId = r.LeagueId,
                               Name = r.PlayerName,
                               TeamName = r.TeamName,
                               GoalsAgainst = r.GoalsAgainst,
                               GoalsFor = r.GoalsFor,
                               Position = r.Position,
                               Points = r.Points,
                               GamesPlayed = r.GamesPlayed,
                               TeamRating = r.TeamRating
                           } 
                           into g
                           select new LeagueTableRow
                           {
                               LeagueId = g.Key.LeagueId,
                               PlayerName = g.Key.Name,
                               TeamName = g.Key.TeamName,
                               GoalsAgainst = g.Key.GoalsAgainst,
                               GoalsFor = g.Key.GoalsFor,
                               Position = g.Key.Position,
                               Points = g.Key.Points,
                               GamesPlayed = g.Key.GamesPlayed,
                               TeamRating = g.Key.TeamRating
                           };
        }
    }
}