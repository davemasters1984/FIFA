using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Interface.Models
{
    public class TopGoalScorers
    {
        public string LeagueId { get; set; }

        public IEnumerable<PlayerWithGoalsScored> Players { get; set; }
    }

    public class PlayerWithGoalsScored
    {
        public int GoalsScored { get; set; }

        public string PlayerId { get; set; }

        public string Face { get; set; }

        public string Name { get; set; }
    }
}
