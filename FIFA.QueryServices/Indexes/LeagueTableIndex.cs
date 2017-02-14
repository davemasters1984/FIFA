using FIFA.Model;
using FIFA.QueryServices.Interface.Models;
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
                    PlayerId = player.Id,
                    PlayerName = player.Name,
                    PlayerFace = player.Face,
                    TeamName = team.TeamName,
                    TeamBadge = team.Badge,
                    TeamId = team.Id,
                    GoalsAgainst = p.GoalsAgainst,
                    GoalsFor = p.GoalsFor,
                    GoalDifference = p.GoalsFor - p.GoalsAgainst,
                    GamesWon = p.GamesWon,
                    GamesDrawn = p.GamesDrawn,
                    GamesLost = p.GamesLost,
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
                               PlayerId = r.PlayerId,
                               PlayerFace = r.PlayerFace,
                               TeamName = r.TeamName,
                               TeamBadge = r.TeamBadge,
                               TeamId = r.TeamId,
                               GoalsAgainst = r.GoalsAgainst,
                               GoalsFor = r.GoalsFor,
                               GoalDifference = r.GoalDifference,
                               GamesWon = r.GamesWon,
                               GamesDrawn = r.GamesDrawn,
                               GamesLost = r.GamesLost,
                               Position = r.Position,
                               Points = r.Points,
                               GamesPlayed = r.GamesPlayed,
                               TeamRating = r.TeamRating
                           } 
                           into g
                           select new LeagueTableRow
                           {
                               LeagueId = g.Key.LeagueId,
                               PlayerId = g.Key.PlayerId,
                               PlayerName = g.Key.Name,
                               PlayerFace = g.Key.PlayerFace,
                               TeamName = g.Key.TeamName,
                               TeamBadge = g.Key.TeamBadge,
                               TeamId = g.Key.TeamId,
                               GoalsAgainst = g.Key.GoalsAgainst,
                               GoalsFor = g.Key.GoalsFor,
                               GoalDifference = g.Key.GoalDifference,
                               GamesWon = g.Key.GamesWon,
                               GamesDrawn = g.Key.GamesDrawn,
                               GamesLost = g.Key.GamesLost,
                               Position = g.Key.Position,
                               Points = g.Key.Points,
                               GamesPlayed = g.Key.GamesPlayed,
                               TeamRating = g.Key.TeamRating
                           };
        }
    }
}