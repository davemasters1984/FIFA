using Raven.Client;
using Raven.Client.Linq;
using System;
using System.Collections.Generic;
using System.Linq;


namespace FIFAData
{
    public class ExampleTwo
    {
        private IDocumentStore _db;

        private IEnumerable<TeamAssignment> GetTeamAssignmentsForNewLeague(IEnumerable<string> particpantNames)
        {
            var teamAssignments = new List<TeamAssignment>();

            if (particpantNames == null)
                return teamAssignments;
            if (!particpantNames.Any())
                return teamAssignments;

            var players = new List<Player>();
            var teams = new List<FifaTeam>();

            using (var session = _db.OpenSession())
            {
                players = session.Query<Player>()
                    .Where(p => p.Name.In(particpantNames))
                    .ToList();

                teams = session.Query<FifaTeam>()
                    .ToList();
            }

            var fourStarAssignments = GetFourStarAssignmentsForNewPlayers(players, teams);
            var rankedAssignments = GetHandicapedAssignmentsForRankedPlayers(players, teams);

            teamAssignments.AddRange(fourStarAssignments);
            teamAssignments.AddRange(rankedAssignments);

            return teamAssignments;
        }

        private IEnumerable<TeamAssignment> GetFourStarAssignmentsForNewPlayers(IEnumerable<Player> players, 
            IEnumerable<FifaTeam> teams)
        {
            var teamAssignments = new List<TeamAssignment>();

            var fourStarTeams = teams
                .Where(t => t.Stars == 4m)
                .ToList();

            var newPlayers = players.Where(p => p.IsNew);

            foreach (var newPlayer in newPlayers)
            {
                if (!newPlayer.IsPlayerBanned)
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
                        Player = newPlayer,
                        Team = randomFourStarTeam,
                    });
                }
            }

            return teamAssignments;
        }

        private IEnumerable<TeamAssignment> GetHandicapedAssignmentsForRankedPlayers(IEnumerable<Player> players, 
            IEnumerable<FifaTeam> teams)
        {
            var teamAssignments = new List<TeamAssignment>();
            var possibleTeamRatings = new List<int>();

            using (var session = _db.OpenSession())
            {
                possibleTeamRatings = session.Query<FifaTeam>()
                    .Where(tr => tr.OverallRating != 0)
                    .Select(t => t.OverallRating)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToList();
            }

            var rankedPlayers = players
                .Where(p => !p.IsNew)
                .OrderByDescending(p => p.OverallScore);

            var ratingsRangeIncrement 
                = (possibleTeamRatings.Max() - (decimal)possibleTeamRatings.Min()) / rankedPlayers.Count();

            var ratingsRangeFrom = (decimal)possibleTeamRatings.Min();
            var ratingsRangeTo = ratingsRangeFrom + ratingsRangeIncrement;

            foreach (var player in rankedPlayers)
            {
                if (!player.IsPlayerBanned)
                {
                    var eligibleRatingsForPlayer = possibleTeamRatings
                        .Where(r => r >= ratingsRangeFrom && r <= ratingsRangeTo)
                        .ToList();

                    var eligibleTeamsForPlayer = teams
                        .Where(t => eligibleRatingsForPlayer.Any(tr => tr == t.OverallRating))
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

                    ratingsRangeFrom = ratingsRangeTo;
                    ratingsRangeTo += ratingsRangeIncrement;
                }
            }

            return teamAssignments;
        }
    }
}
