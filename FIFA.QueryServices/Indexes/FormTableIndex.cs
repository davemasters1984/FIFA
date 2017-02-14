using FIFA.Model;
using FIFA.QueryServices.Interface.Models;
using Raven.Client.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Indexes
{
    public class FormTableIndex : AbstractIndexCreationTask<League, FormTableRow>
    {
        public FormTableIndex()
        {
            MaxIndexOutputsPerDocument = 500;

            Map =
                leagues =>
                from league in leagues
                from p in league.Participants
                let team = LoadDocument<Team>(p.TeamId)
                let player = LoadDocument<Player>(p.PlayerId)

                select new FormTableRow
                {
                    LeagueId = league.Id,
                    PlayerFace = player.Face,
                    PlayerId = player.Id,
                    TeamBadge = team.Badge,
                    TeamId = team.Id,
                    Results = league.Fixtures
                              .Where(f => f.Result != null)
                              .Where(f => f.HomePlayerId == p.PlayerId || f.AwayPlayerId == p.PlayerId)
                              .OrderByDescending(f => f.Result.Date)
                              .Take(6)
                              .Select(f => new Res
                              {
                                  HomePlayerId = f.HomePlayerId,
                                  AwayPlayerId = f.AwayPlayerId,
                                  HomePoints = f.Result.HomePoints,
                                  AwayPoints = f.Result.AwayPoints
                              }),
                    TotalPoints = 1,
                };

            Reduce =
                results => from r in results
                           group r by new
                           {
                               LeagueId = r.LeagueId,
                               PlayerId = r.PlayerId,
                               PlayerFace = r.PlayerFace,
                               TeamBadge = r.TeamBadge,
                               TeamId = r.TeamId,
                               Results = r.Results,
                           }
                           into g
                           select new FormTableRow
                           {
                               LeagueId = g.Key.LeagueId,
                               PlayerId = g.Key.PlayerId,
                               PlayerFace = g.Key.PlayerFace,
                               TeamBadge = g.Key.TeamBadge,
                               TeamId = g.Key.TeamId,
                               Results = g.Key.Results,
                               TotalPoints = g.Key.Results.Where(r => r.HomePlayerId == g.Key.PlayerId).Sum(r => r.HomePoints) +
                                             g.Key.Results.Where(r => r.AwayPlayerId == g.Key.PlayerId).Sum(r => r.AwayPoints)
                           };
        }
    }
}
