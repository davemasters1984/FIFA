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

            if (IsNoParticipants(particpantNames))
                return teamAssignments;

            var players = GetAllPlayersParticipating(particpantNames);
            var teams = GetAllTeams();

            var fourStarAssignments = GetFourStarAssignmentsForNewPlayers(players, teams);
            var rankedAssignments = GetHandicapedAssignmentsForRankedPlayers(players, teams);

            teamAssignments.AddRange(fourStarAssignments);
            teamAssignments.AddRange(rankedAssignments);

            return teamAssignments;
        }

        private bool IsNoParticipants(IEnumerable<string> participants)
        {
            if (participants == null)
                return true;
            if (!participants.Any())
                return true;

            return false;
        }

        private IEnumerable<TeamAssignment> GetFourStarAssignmentsForNewPlayers(IEnumerable<Player> players, 
            IEnumerable<FifaTeam> teams)
        {
            var assigner = new FourStarTeamAssigner(players, teams);

            return assigner.GetAssignments();
        }

        private IEnumerable<TeamAssignment> GetHandicapedAssignmentsForRankedPlayers(IEnumerable<Player> players, 
            IEnumerable<FifaTeam> teams)
        {
            var possibleTeamRatings = GetPossibleTeamRatings();

            var assigner = new HandicappedTeamAssigner(players, teams, possibleTeamRatings);

            return assigner.GetAssignments();
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
    }
}
