using Raven.Client;
using Raven.Client.Linq;
using System;
using System.Collections.Generic;
using System.Linq;


namespace FIFAData
{
    public class ExampleThree
    {
        private IDocumentStore _db;

        public IEnumerable<TeamAssignment> GetTeamAssignmentsForNewLeague(IEnumerable<string> particpantNames)
        {
            var teamAssignments = new List<TeamAssignment>();

            if (particpantNames == null)
                return teamAssignments;
            if (!particpantNames.Any())
                return teamAssignments;

            var players = GetAllPlayersParticipating(particpantNames);
            var teams = GetAllTeams();

            var fourStarAssignments = GetFourStarAssignmentsForNewPlayers(players, teams);
            var rankedAssignments = GetHandicapedAssignmentsForRankedPlayers(players, teams);

            teamAssignments.AddRange(fourStarAssignments);
            teamAssignments.AddRange(rankedAssignments);

            return teamAssignments;
        }

        private IEnumerable<TeamAssignment> GetFourStarAssignmentsForNewPlayers(IEnumerable<Player> players, IEnumerable<FifaTeam> teams)
        {
            var teamAssignments = new List<TeamAssignment>();

            var fourStarTeams = teams
                .Where(t => t.Stars == 4m)
                .ToList();

            var newPlayers = players.Where(p => p.IsNew);

            foreach (var newPlayer in newPlayers)
            {
                if (newPlayer.IsPlayerBanned)
                    continue;
                if (newPlayer.SomeOtherFakeCondition)
                    continue;

                var randomFourStarTeamAssignment = GetRandomFourStarTeamAssignment(newPlayer, fourStarTeams, teamAssignments);

                teamAssignments.Add(randomFourStarTeamAssignment);
            }

            return teamAssignments;
        }

        private IEnumerable<TeamAssignment> GetHandicapedAssignmentsForRankedPlayers(IEnumerable<Player> players, IEnumerable<FifaTeam> teams)
        {
            var teamAssignments = new List<TeamAssignment>();
            var possibleTeamRatings = GetPossibleTeamRatings();

            var rankedPlayers = players.Where(p => !p.IsNew);

            var ratingsRangeIncrement = (possibleTeamRatings.Max() - (decimal)possibleTeamRatings.Min()) / rankedPlayers.Count();
            var ratingsRangeFrom = (decimal)possibleTeamRatings.Min();
            var ratingsRangeTo = ratingsRangeFrom + ratingsRangeIncrement;

            foreach (var player in rankedPlayers)
            {
                if (player.IsPlayerBanned)
                    continue;
                if (player.SomeOtherFakeCondition)
                    continue;

                var handicappedTeamAssignment = GetHandicappedTeamAssignment(player, teams, possibleTeamRatings, ratingsRangeFrom, ratingsRangeTo);

                teamAssignments.Add(handicappedTeamAssignment);

                ratingsRangeFrom = ratingsRangeTo;
                ratingsRangeTo += ratingsRangeIncrement;
            }

            return teamAssignments;
        }

        private IEnumerable<Player> GetAllPlayersParticipating(IEnumerable<string> particpantNames)
        {
            using (var session = _db.OpenSession())
            {
                return session.Query<Player>()
                    .Where(p => p.Name.In(particpantNames))
                    .ToList();
            }
        }

        private IEnumerable<FifaTeam> GetAllTeams()
        {
            using (var session = _db.OpenSession())
            {
                return session.Query<FifaTeam>()
                    .ToList();
            }
        }

        private IEnumerable<int> GetPossibleTeamRatings()
        {
            using (var session = _db.OpenSession())
            {
                return session.Query<FifaTeam>()
                    .Where(tr => tr.OverallRating != 0)
                    .Select(t => t.OverallRating)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToList();
            }
        }

        private TeamAssignment GetRandomFourStarTeamAssignment(Player newPlayer, List<FifaTeam> fourStarTeams, List<TeamAssignment> existingAssignments)
        {
            var eligableTeams
                = fourStarTeams.Where(t => NotInExistingAssignments(t, existingAssignments))
                    .ToList();

            var rnd = new Random();
            var randomTeamIndex = rnd.Next(eligableTeams.Count - 1);

            var randomFourStarTeam = eligableTeams[randomTeamIndex];

            return new TeamAssignment { Player = newPlayer, Team = randomFourStarTeam, IsNewPlayer = true };
        }

        private bool NotInExistingAssignments(FifaTeam teamToTest, IEnumerable<TeamAssignment> existingAssignments)
        {
            return !existingAssignments.Any(a => a.Team.TeamName == teamToTest.TeamName);
        }

        private TeamAssignment GetHandicappedTeamAssignment(Player player, IEnumerable<FifaTeam> teams, IEnumerable<int> possibleTeamRatings, decimal currentFrom, decimal currentTo)
        {
            var teamRatingsForPlayer = possibleTeamRatings
                .Where(r => r >= currentFrom && r <= currentTo)
                .ToList();

            var possibleTeamsForPlayer = teams.Where(t => t.OverallRating.In(teamRatingsForPlayer)).ToList();

            var rnd = new Random();
            var randomTeamIndex = rnd.Next(possibleTeamsForPlayer.Count - 1);

            var randomTeamFromPossibleRatings = possibleTeamsForPlayer[randomTeamIndex];

            return new TeamAssignment { Player = player, PossibleTeamRatings = teamRatingsForPlayer, Team = randomTeamFromPossibleRatings, IsNewPlayer = false };

        }
    }
}
