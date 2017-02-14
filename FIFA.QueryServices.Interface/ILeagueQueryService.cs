using FIFA.QueryServices.Interface.Models;
using System.Collections.Generic;

namespace FIFA.QueryServices.Interface
{
    public interface ILeagueQueryService
    {
        string GetCurrentLeagueId();

        IEnumerable<LeagueTableRow> GetLeagueTable(string leagueId);

        IEnumerable<LeagueTableRow> GetCurrentLeagueTable();

        IEnumerable<FixtureSummary> GetFixturesForPlayer(string leagueId, string playerId);

        IEnumerable<FixtureSummary> GetFixturesForPlayerByFace(string leagueId, string face);

        IEnumerable<ResultSummary> GetResultsForPlayerByFace(string leagueId, string face);

        IEnumerable<FormTableRow> GetFormTable(string leagueId);

        PlayerPositionHistoryComparison GetPlayerPositionHistoryComparisonForCurrentLeague(string playerOneId, string playerTwoId);
    }
}
