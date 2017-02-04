using System.Collections.Generic;

namespace FIFA.Model
{
    public class TeamAssignment
    {
        public Team Team { get; set; }

        public Player Player { get; set; }

        public IEnumerable<int> EligibleTeamRatings { get; set; }
    }
}
