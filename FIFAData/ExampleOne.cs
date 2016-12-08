using Raven.Client;
using Raven.Client.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FIFAData
{
    public class ExampleOne
    {
        private IDocumentStore _db;

        private IEnumerable<TeamAssignment> GetTeamAssignmentsForNewLeague(IEnumerable<string> particpantNames)
        {
            var teamAssignments = new List<TeamAssignment>();

            if (particpantNames != null && particpantNames.Any())
            {
                var players = new List<Player>();
                var teams = new List<FifaTeam>();
                var possibleTeamRatings = new List<int>();

                using (var session = _db.OpenSession())
                {
                    players = session.Query<Player>()
                        .Where(p => p.Name.In(particpantNames))
                        .ToList();

                    teams = session.Query<FifaTeam>()
                        .ToList();

                    possibleTeamRatings = session.Query<FifaTeam>()
                        .Where(tr => tr.OverallRating != 0)
                        .Select(t => t.OverallRating)
                        .Distinct()
                        .OrderBy(r => r)
                        .ToList();
                }

                var fourStarTeams = teams
                    .Where(t => t.Stars == 4m)
                    .ToList();

                var ratingsRangeIncrement = (possibleTeamRatings.Max() - (decimal)possibleTeamRatings.Min()) / players.Count();
                var currentFrom = (decimal)possibleTeamRatings.Min();
                var currentTo = currentFrom + ratingsRangeIncrement;

                foreach (var player in players)
                {
                    if (!player.IsPlayerBanned && !player.SomeOtherFakeCondition)
                    {
                        if (player.IsNew)
                        {
                            var eligableTeams
                                = teams.Where(t => !t.TeamName.In(teamAssignments.Select(ta => ta.Team.TeamName)))
                                    .ToList();

                            var rnd = new Random();
                            var randomTeamIndex = rnd.Next(eligableTeams.Count - 1);

                            var randomFourStarTeam = eligableTeams[randomTeamIndex];

                            teamAssignments.Add(new TeamAssignment { Player = player, Team = randomFourStarTeam, IsNewPlayer = true });
                        }
                        else
                        {
                            var teamRatingsForPlayer = possibleTeamRatings
                                .Where(r => r >= currentFrom && r <= currentTo)
                                .ToList();

                            var possibleTeamsForPlayer = teams.Where(t => t.OverallRating.In(teamRatingsForPlayer)).ToList();

                            var rnd = new Random();
                            var randomTeamIndex = rnd.Next(teams.Count - 1);

                            var randomTeamFromPossibleRatings = possibleTeamsForPlayer[randomTeamIndex];

                            teamAssignments.Add(new TeamAssignment { Player = player, PossibleTeamRatings = teamRatingsForPlayer, Team = randomTeamFromPossibleRatings, IsNewPlayer = false });

                            currentFrom = currentTo;
                            currentTo += ratingsRangeIncrement;
                        }
                    }
                }
            }

            return teamAssignments;
        }
    }
}
