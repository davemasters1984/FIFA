using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.Model.Services
{
    public class CreateLeagueArgs
    {
        public string Name { get; set; }

        public IEnumerable<Team> Teams { get; set; }

        public IEnumerable<Player> Players { get; set; }

        public IEnumerable<int> PossibleTeamRatings { get; set; }

        public IEnumerable<League> PreviousLeagues { get; set; }
    }
}
