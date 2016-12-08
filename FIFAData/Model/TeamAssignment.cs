using System.Collections.Generic;

namespace FIFAData
{
    public class TeamAssignment
    {
        public FifaTeam Team { get; set; }

        public Player Player { get; set; }

        public IEnumerable<int> EligibleTeamRatings { get; set; }
    }

}
