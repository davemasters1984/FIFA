using FIFA.QueryServices.Interface.Models;
using System.Collections.Generic;

namespace FIFA.QueryServices.Interface
{
    public interface ILeagueQueryService
    {
        string GetCurrentLeagueId();

        IEnumerable<LeagueTableRow> GetLeagueTable(string leagueId);

        IEnumerable<LeagueTableRow> GetLeagueTableWaitForIndex(string leagueId);

        IEnumerable<LeagueTableRow> GetCurrentLeagueTable();

        IEnumerable<FixtureSummary> GetFixturesForPlayer(string leagueId, string playerId);

        IEnumerable<FixtureSummary> GetFixturesForPlayerByFace(string leagueId, string face);

        IEnumerable<ResultSummary> GetResultsForPlayerByFace(string leagueId, string face);

        IEnumerable<FormTableRow> GetFormTable(string leagueId);

        IEnumerable<PlayerPositionHistory> GetPostionHistoryForPlayers(string leagueId, IEnumerable<string> playerIds);

        PlayerPositionHistoryComparison GetPlayerPositionHistoryComparisonForCurrentLeague(string playerOneId, string playerTwoId);

        CurrentLeagueAndPlayerIds GetCurrentLeagueAndPlayerIds(string playerFaceOne, string playerFaceTwo);

        IEnumerable<ResultSummary> GetHeadToHeadResults(string leagueId, string faceOne, string faceTwo);
    }
}
