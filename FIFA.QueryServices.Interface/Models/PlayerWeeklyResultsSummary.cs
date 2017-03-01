using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Interface.Models
{
    public class PlayerWeeklyResultsSummary
    {
        public string PlayerId { get; set; }

        public int GoalsScored { get; set; }

        public int GoalsConceded { get; set; }

        public int Points { get; set; }

        public int GamesPlayed { get; set; }
    }
}
