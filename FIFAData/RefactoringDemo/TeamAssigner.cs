using Raven.Client;
using Raven.Client.Linq;
using System.Collections.Generic;
using System.Linq;

namespace FIFAData
{
    public class TeamAssigner
    {
        private readonly IDocumentStore _database;
        private readonly List<TeamAssignment> _assignments = new List<TeamAssignment>();
        private IEnumerable<FifaTeam> _teams;
        private IEnumerable<Player> _players;
        private IEnumerable<int> _possibleTeamRankings;

        public TeamAssigner(IDocumentStore database)
        {
            _database = database;
        }

        public IEnumerable<TeamAssignment> GetAssignments(IEnumerable<string> particpantNames)
        {
            FetchRequiredData(particpantNames);

            AssignFourStarTeamsToNewPlayers();

            AssignHandicappedTeamsToRankedPlayers();

            return _assignments;
        }

        private void FetchRequiredData(IEnumerable<string> particpantNames)
        {
            FetchTeams();

            FetchPossibleTeamRatings();

            FetchPlayersMatchingParticipantNames(particpantNames);
        }

        private void FetchTeams()
        {
            using (var session = _database.OpenSession())
            {
                _teams = session.Query<FifaTeam>()
                    .ToList();
            }
        }

        private void FetchPlayersMatchingParticipantNames(IEnumerable<string> particpantNames)
        {
            using (var session = _database.OpenSession())
            {
                _players = session.Query<Player>()
                    .Where(p => p.Name.In(particpantNames))
                    .ToList();
            }
        }

        private IEnumerable<int> FetchPossibleTeamRatings()
        {
            using (var session = _database.OpenSession())
            {
                return session.Query<FifaTeam>()
                    .Where(tr => tr.OverallRating != 0)
                    .Select(t => t.OverallRating)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToList();
            }
        }

        private void AssignFourStarTeamsToNewPlayers()
        {
            var fourStarTeamAssigner = new FourStarTeamAssigner(_players, _teams);

            var fourStarAssignments = fourStarTeamAssigner.GetAssignments();

            _assignments.AddRange(fourStarAssignments);
        }

        private void AssignHandicappedTeamsToRankedPlayers()
        {
            var handicapedAssigner
                = new HandicappedTeamAssigner(_players, _teams, _possibleTeamRankings);

            var handicappedAssignments = handicapedAssigner.GetAssignments();

            _assignments.AddRange(handicappedAssignments);
        }
    }
}
