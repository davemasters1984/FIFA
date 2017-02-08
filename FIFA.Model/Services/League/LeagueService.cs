using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.Model.Services
{
    public class LeagueService : ILeagueService
    {
        public League CreateNewLeague(CreateLeagueArgs args)
        {
            var teamAssigner = new TeamAssigner(args);

            var assignments = teamAssigner.GetAssignments();

            var newLeague = new League();

            newLeague.CreatedDate = DateTime.Now;
            newLeague.Participants = new List<Participant>();

            foreach (var assignment in assignments)
            {
                newLeague.Participants.Add(new Participant
                {
                    PlayerId = assignment.Player.Id,
                    TeamId = assignment.Team.Id,
                    EligibleTeamRatings = assignment.EligibleTeamRatings
                });
            }

            return newLeague;
        }
    }
}
