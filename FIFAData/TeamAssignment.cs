using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFAData
{
    public class TeamAssignment
    {
        public FifaTeam Team { get; set; }

        public Player Player { get; set; }

        public IEnumerable<int> PossibleTeamRatings { get; set; }

        public bool IsNewPlayer { get; set; }
    }

}
