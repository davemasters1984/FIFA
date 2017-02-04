using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.Model
{
    public class Participant
    {
        public string ParticipantId { get; set; }

        public string TeamId { get; set; }

        public IEnumerable<int> EligibleTeamRatings { get; set; }

        public int Position { get; set; }

        public int GamesPlayed { get; set; }

        public int Points { get; set; }

        public int GoalsFor { get; set; }

        public int GoalsAgainst { get; set; }
    }
}
