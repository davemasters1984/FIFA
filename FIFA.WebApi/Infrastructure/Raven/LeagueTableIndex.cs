using FIFA.Model;
using FIFA.WebApi.Models;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static FIFA.WebApi.Infrastructure.LeagueTableIndex;

namespace FIFA.WebApi.Infrastructure
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
                    GamesPlayed = p.GamesPlayed
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
                               GamesPlayed = r.GamesPlayed
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
                               GamesPlayed = g.Key.GamesPlayed
                           };
        }
    }
}