using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Interface
{
    public class GenerateStatisticsForPeriodArgs
    {
        public string LeagueId { get; set; }

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }
    }
}
