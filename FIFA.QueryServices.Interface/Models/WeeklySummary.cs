using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Interface.Models
{
    public class WeeklySummary
    {
        public WeeklyStatistic PlayerWithMostGoals { get; set; }

        public WeeklyStatistic PlayerWithMostGoalsConceded { get; set; }

        public WeeklyStatistic PlayerWithMostGamesPlayed { get; set; }

        public WeeklyStatistic PlayerWithMostPoints { get; set; }

        public WeeklyStatistic PlayerWithLeastPoints { get; set; }
    }

    public class WeeklyStatistic
    {
        public string PlayerId { get; set; }

        public string Face { get; set; }

        public int GamesPlayed { get; set; }

        public int KeyStat { get; set; }
    }

}
