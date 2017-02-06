using System.Collections.Generic;

namespace FIFA.Model
{
    public class Participant
    {
        public string PlayerId { get; set; }

        public string TeamId { get; set; }

        public IEnumerable<int> EligibleTeamRatings { get; set; }

        public int Position { get; set; }

        public int GamesPlayed { get; set; }

        public int Points { get; set; }

        public int GoalsFor { get; set; }

        public int GoalsAgainst { get; set; }

        public void PostResultAsHomePlayer(Result result)
        {
            Points += result.HomePoints;
            GoalsFor += result.HomePlayerGoals;
            GoalsAgainst += result.AwayPlayerGoals;
            GamesPlayed++;
        }

        public void PostResultAsAwayPlayer(Result result)
        {
            Points += result.AwayPoints;
            GoalsFor += result.AwayPlayerGoals;
            GoalsAgainst += result.HomePlayerGoals;
            GamesPlayed++;
        }
    }
}
