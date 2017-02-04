using FIFA.Model;
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
        public class LeagueTableRow
        {
            public string PlayerName { get; set; }

            public string TeamName { get; set; }

            public int Position { get; set; }

            public int GamesPlayed { get; set; }

            public int Points { get; set; }

            public int GoalsFor { get; set; }

            public int GoalsAgainst { get; set; }
        }

        public LeagueTableIndex()
        {
            Map =
                leagues =>
                from league in leagues
                from p in league.Participants
                let team = LoadDocument<Team>(p.TeamId)
                let player = LoadDocument<Player>(p.ParticipantId)
                select new LeagueTableRow
                {
                    PlayerName = player.Name,
                    TeamName = team.TeamName,
                    GoalsAgainst = p.GoalsAgainst,
                    GoalsFor = p.GoalsFor,
                    Position = p.Position,
                    Points = p.Points,
                    GamesPlayed = p.GamesPlayed
                };

            Stores.Add(x => x.PlayerName, FieldStorage.Yes);
            Stores.Add(x => x.TeamName, FieldStorage.Yes);

            Reduce =
                results => from r in results
                           group r by new
                           {
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