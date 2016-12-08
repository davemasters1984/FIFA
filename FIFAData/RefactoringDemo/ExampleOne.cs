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

                var rankedPlayers = players
                    .Where(p => !p.IsNew)
                    .OrderByDescending(p => p.OverallScore);

                var ratingsRangeIncrement 
                    = (possibleTeamRatings.Max() - (decimal)possibleTeamRatings.Min()) / rankedPlayers.Count();

                var ratingRangeFrom = (decimal)possibleTeamRatings.Min();
                var ratingRangeTo = ratingRangeFrom + ratingsRangeIncrement;

                foreach (var player in players)
                {
                    if (!player.IsPlayerBanned)
                    {
                        if (player.IsNew)
                        {
                            var eligibleTeamsForPlayer
                                = fourStarTeams.Where(fst =>
                                    !teamAssignments.Any(ta => ta.Team.TeamName == fst.TeamName))
                                        .ToList();

                            var rnd = new Random();
                            var randomTeamIndex = rnd.Next(eligibleTeamsForPlayer.Count - 1);

                            var randomFourStarTeam = eligibleTeamsForPlayer[randomTeamIndex];

                            teamAssignments.Add(new TeamAssignment
                            {
                                Player = player,
                                Team = randomFourStarTeam
                            });
                        }
                        else
                        {
                            var eligibleRatingsForPlayer = possibleTeamRatings
                                .Where(r => r >= ratingRangeFrom && r <= ratingRangeTo)
                                .ToList();

                            var eligibleTeamsForPlayer
                                = teams.Where(t => eligibleRatingsForPlayer.Any(er => er == t.OverallRating))
                                       .ToList();

                            var rnd = new Random();
                            var randomTeamIndex = rnd.Next(eligibleTeamsForPlayer.Count - 1);

                            var randomTeamFromPossibleRatings = eligibleTeamsForPlayer[randomTeamIndex];

                            teamAssignments.Add(new TeamAssignment
                            {
                                Player = player,
                                EligibleTeamRatings = eligibleRatingsForPlayer,
                                Team = randomTeamFromPossibleRatings
                            });

                            ratingRangeFrom = ratingRangeTo;
                            ratingRangeTo += ratingsRangeIncrement;
                        }
                    }
                }
            }

            return teamAssignments;
        }
    }
}
