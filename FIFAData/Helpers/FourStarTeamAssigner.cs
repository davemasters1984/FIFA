using System;
using System.Collections.Generic;
using System.Linq;

namespace FIFAData
{
    public class FourStarTeamAssigner
    {
        private readonly IEnumerable<Player> _newPlayers;
        private readonly IEnumerable<FifaTeam> _fourStarTeams;
        private readonly List<TeamAssignment> _assignments 
            = new List<TeamAssignment>();

        public FourStarTeamAssigner(IEnumerable<Player> players, 
            IEnumerable<FifaTeam> teams)
        {
            _fourStarTeams = teams.Where(t => t.Stars == 4m);
            _newPlayers = players.Where(p => p.IsNew);
        }

        public IEnumerable<TeamAssignment> GetAssignments()
        {
            ClearAnyPreviousAssignments();

            AssignFourStarTeamsToNewPlayers();

            return _assignments;
        }

        private void ClearAnyPreviousAssignments()
        {
            _assignments.Clear();
        }

        private void AssignFourStarTeamsToNewPlayers()
        {
            foreach (var newPlayer in _newPlayers)
            {
                if (newPlayer.IsPlayerBanned)
                    continue;

                var randomFourStarTeamAssignment = GetRandomFourStarTeamAssignment(newPlayer);

                _assignments.Add(randomFourStarTeamAssignment);
            }
        }

        private TeamAssignment GetRandomFourStarTeamAssignment(Player newPlayer)
        {
            var eligibleTeamsForPlayer
                = _fourStarTeams.Where(TeamNotInExistingAssignments)
                    .ToList();

            var randomEligibleTeam 
                = GetRandomTeamFromEligibleTeams(eligibleTeamsForPlayer);

            return new TeamAssignment
            {
                Player = newPlayer,
                Team = randomEligibleTeam,
            };
        }

        private FifaTeam GetRandomTeamFromEligibleTeams(List<FifaTeam> eligibleTeams)
        {
            var rnd = new Random();
            var randomTeamIndex = rnd.Next(eligibleTeams.Count - 1);

            return eligibleTeams[randomTeamIndex];
        }

        private bool TeamNotInExistingAssignments(FifaTeam teamToTest)
        {
            return !_assignments.Any(a => a.Team.TeamName == teamToTest.TeamName);
        }
    }
}
