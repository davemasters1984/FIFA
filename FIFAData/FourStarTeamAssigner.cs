using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFAData
{
    public class FourStarTeamAssigner
    {
        private readonly IEnumerable<Player> _newPlayers;
        private readonly IEnumerable<FifaTeam> _fourStarTeams;
        private readonly List<TeamAssignment> _assignments = new List<TeamAssignment>();

        public FourStarTeamAssigner(IEnumerable<Player> players, IEnumerable<FifaTeam> teams)
        {
            _fourStarTeams = teams.Where(t => t.Stars == 4m);
            _newPlayers = players.Where(p => p.IsNew);
        }

        public IEnumerable<TeamAssignment> Assign()
        {
            _assignments.Clear();

            foreach (var newPlayer in _newPlayers)
            {
                if (newPlayer.IsPlayerBanned)
                    continue;
                if (newPlayer.SomeOtherFakeCondition)
                    continue;

                var randomFourStarTeamAssignment = GetRandomFourStarTeamAssignment(newPlayer);

                _assignments.Add(randomFourStarTeamAssignment);
            }

            return _assignments;
        }

        private TeamAssignment GetRandomFourStarTeamAssignment(Player newPlayer)
        {
            var eligableTeams
                = _fourStarTeams.Where(NotInExistingAssignments)
                    .ToList();

            var rnd = new Random();
            var randomTeamIndex = rnd.Next(eligableTeams.Count - 1);

            var randomFourStarTeam = eligableTeams[randomTeamIndex];

            return new TeamAssignment { Player = newPlayer, Team = randomFourStarTeam, IsNewPlayer = true };
        }

        private bool NotInExistingAssignments(FifaTeam teamToTest)
        {
            return !_assignments.Any(a => a.Team.TeamName == teamToTest.TeamName);
        }
    }
}
