using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Interface.Models
{
    public class LeagueStatisticsForPeriod
    {
        public string Id { get; set; }

        public string LeagueId { get; set; }

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        public int Days
        {
            get { return (int)PeriodEnd.Subtract(PeriodStart).TotalDays; }
        }

        public DateTime DateCreated { get; set; }

        public List<PlayerStatisticsSummary> PlayerStatistics { get; set; }
    }
}
