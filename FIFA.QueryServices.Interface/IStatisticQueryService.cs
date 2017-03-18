using FIFA.QueryServices.Interface.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Interface
{
    public interface IStatisticQueryService
    {
        LeagueStatisticsForPeriod GenerateStatisticsForPeriod (GenerateStatisticsForPeriodArgs args);

        TopGoalScorers GetCurrentTopGoalScorersForLeague(string leagueId);

        StatisticSummary GetWeeklySummary(string leagueId);

        IEnumerable<PredictedLeagueTableRow> GetPredictedTable(string leagueId);
    }
}
