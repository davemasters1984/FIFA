using Raven.Client;
using Raven.Client.Linq;
using System.Collections.Generic;
using System.Linq;


namespace FIFAData
{
    public class ExampleFour
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
            var assigner = new FourStarTeamAssigner(players, teams);

            return assigner.Assign();
        }

        private IEnumerable<TeamAssignment> GetHandicapedAssignmentsForRankedPlayers(IEnumerable<Player> players, IEnumerable<FifaTeam> teams)
        {
            var possibleTeamRatings = GetPossibleTeamRatings();

            var assigner = new HandicappedTeamAssigner(players, teams, possibleTeamRatings);

            return assigner.Assign();
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
    }

    private class TeamAssignmentService
    {
        private readonly IDocumentStore _database;
        private IEnumerable<FifaTeam> _teams;
        private IEnumerable<Player> _players;
        private IEnumerable<TeamAssignment> _assignments;

        public TeamAssignmentService(IDocumentStore database)
        {
            _database = database;
        }

        public IEnumerable<TeamAssignment> Assign(IEnumerable<string> particpantNames)
        {
            GetAllTeams();
            GetPlayersFromParticipantNames(particpantNames);

            return _assignments;
        }

        private void GetAllTeams()
        {
            using (var session = _database.OpenSession())
            {
                _teams = session.Query<FifaTeam>()
                    .ToList();
            }
        }

        private void GetPlayersFromParticipantNames(IEnumerable<string> particpantNames)
        {
            using (var session = _database.OpenSession())
            {
                _players = session.Query<Player>()
                    .Where(p => p.Name.In(particpantNames))
                    .ToList();
            }
        }

    }
}
