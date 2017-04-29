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
            var participants = GenerateParticipants(args);

            var fixtures = GenerateFixtures(participants);

            var newLeague = new League(args.Name, DateTime.Now, participants, fixtures);

            return newLeague;
        }

        private List<Participant> GenerateParticipants(CreateLeagueArgs args)
        {
            var teamAssigner = new TeamAssigner(args);

            var assignments = teamAssigner.GetAssignments();

            var newLeague = new League();

            newLeague.CreatedDate = DateTime.Now;
            var participants = new List<Participant>();

            foreach (var assignment in assignments)
            {
                participants.Add(new Participant
                {
                    PlayerId = assignment.Player.Id,
                    TeamId = assignment.Team.Id,
                    EligibleTeamRatings = assignment.EligibleTeamRatings
                });
            }

            return participants;
        }

        private List<Fixture> GenerateFixtures(List<Participant> participants)
        {
            var fixtureGenerator = new FixtureGenerator(participants);

            return fixtureGenerator.GenerateFixtures();
        }
    }
}
