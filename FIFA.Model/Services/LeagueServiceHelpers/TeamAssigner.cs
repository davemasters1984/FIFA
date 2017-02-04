using System.Collections.Generic;

namespace FIFA.Model.Assigners
{
    public class TeamAssigner
    {
        private readonly List<TeamAssignment> _assignments = new List<TeamAssignment>();
        private IEnumerable<Team> _teams;
        private IEnumerable<Player> _players;
        private IEnumerable<int> _possibleTeamRankings;
        private IEnumerable<League> _previousLeagues;

        public TeamAssigner(CreateLeagueArgs args)
        {
            _teams = args.Teams;
            _players = args.Players;
            _possibleTeamRankings = args.PossibleTeamRatings;
            _previousLeagues = args.PreviousLeagues;
        }

        public IEnumerable<TeamAssignment> GetAssignments()
        {
            ClearAnyPreviousAssignments();

            AssignFourStarTeamsToNewPlayers();

            AssignHandicappedTeamsToRankedPlayers();

            return _assignments;
        }

        private void ClearAnyPreviousAssignments()
        {
            _assignments.Clear();
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
                = new HandicappedTeamAssigner(_players, _teams, _possibleTeamRankings, _previousLeagues);

            var handicappedAssignments = handicapedAssigner.GetAssignments();

            _assignments.AddRange(handicappedAssignments);
        }
    }
}
