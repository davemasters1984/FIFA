using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Interface.Models
{
    public class StatisticSummary
    {
        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        public string LeagueId { get; set; }

        public PlayerStatistic PlayerWithMostGoals { get; set; }

        public PlayerStatistic PlayerWithMostGoalsConceded { get; set; }

        public PlayerStatistic PlayerWithMostGamesPlayed { get; set; }

        public PlayerStatistic PlayerWithLeastGamesPlayed { get; set; }

        public PlayerStatistic PlayerWithMostPoints { get; set; }

        public PlayerStatistic PlayerWithLeastPoints { get; set; }

        public PlayerStatistic PlayerWithBestAttack { get; set; }

        public PlayerStatistic PlayerWithWorstAttack { get; set; }

        public PlayerStatistic PlayerWithBestDefence { get; set; }

        public PlayerStatistic PlayerWithWorstDefence { get; set; }
    }

    public class PlayerStatistic
    {
        public string PlayerId { get; set; }

        public string Face { get; set; }

        public int GamesPlayed { get; set; }

        public double KeyStat { get; set; }
    }

}
