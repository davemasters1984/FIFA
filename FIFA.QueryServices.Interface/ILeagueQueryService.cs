using FIFA.QueryServices.Interface.Models;
using System.Collections.Generic;

namespace FIFA.QueryServices.Interface
{
    public interface ILeagueQueryService
    {
        IEnumerable<LeagueTableRow> GetLeagueTable(string leagueId);

        IEnumerable<LeagueTableRow> GetCurrentLeagueTable();
    }
}
