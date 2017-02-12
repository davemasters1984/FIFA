using FIFA.Model;
using FIFA.QueryServices.Indexes;
using FIFA.QueryServices.Interface;
using FIFA.QueryServices.Interface.Models;
using Raven.Client;
using System.Collections.Generic;
using System.Linq;
using System;

namespace FIFA.QueryServices.Services
{
    public class LeagueQueryService : ILeagueQueryService
    {
        private IDocumentStore _documentStore;

        public LeagueQueryService(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public string GetCurrentLeagueId()
        {
            using (var session = _documentStore.OpenSession())
            {
                var leagueId = session.Query<League>()
                    .OrderByDescending(l => l.CreatedDate)
                    .Select(l => l.Id)
                    .FirstOrDefault();

                return leagueId;
            }
        }

        public IEnumerable<ResultSummary> GetResultsForPlayerByFace(string leagueId, string face)
        {
            using (var session = _documentStore.OpenSession())
            {
                var results = session.Query<ResultSummary, ResultsIndex>()
                    .Where(l => l.LeagueId == leagueId)
                    .Where(l => l.HomePlayerFace == face || l.AwayPlayerFace == face)
                    .ToList();

                return results;
            }
        }

        public IEnumerable<LeagueTableRow> GetCurrentLeagueTable()
        {
            using (var session = _documentStore.OpenSession())
            {
                var leagueId = session.Query<League>()
                    .OrderByDescending(l => l.CreatedDate)
                    .Select(l => l.Id)
                    .FirstOrDefault();

                return GetLeagueTable(session, leagueId);
            }
        }

        public IEnumerable<FixtureSummary> GetFixturesForPlayer(string leagueId, string playerId)
        {
            using (var session = _documentStore.OpenSession())
            {
                var fixtures = session.Query<FixtureSummary, FixturesIndex>()
                    .Where(l => l.LeagueId == leagueId)
                    .Where(l => l.HomePlayerId == playerId || l.AwayPlayerId == playerId)
                    .ToList();

                return fixtures;
            }
        }

        public IEnumerable<FixtureSummary> GetFixturesForPlayerByFace(string leagueId, string face)
        {
            using (var session = _documentStore.OpenSession())
            {
                var fixtures = session.Query<FixtureSummary, FixturesIndex>()
                    .Where(l => l.LeagueId == leagueId)
                    .Where(l => l.HomePlayerFace == face || l.AwayPlayerFace == face)
                    .ToList();

                return fixtures;
            }
        }

        public IEnumerable<LeagueTableRow> GetLeagueTable(string leagueId)
        {
            using (var session = _documentStore.OpenSession())
                return GetLeagueTable(session, leagueId);
        }

        private IEnumerable<LeagueTableRow> GetLeagueTable(IDocumentSession session, string leagueId)
        {
            var leagueTable
                = session.Query<LeagueTableRow, LeagueTableIndex>()
                    .Where(l => l.LeagueId == leagueId)
                    .OrderByDescending(l => l.Points)
                    .ThenByDescending(l => l.GoalDifference)
                    .ThenByDescending(l => l.GoalsFor)
                    .ToList();

            var lastSnapshot
                = session.Query<LeagueTableSnapshot>()
                    .Where(l => l.LeagueId == leagueId)
                    .Where(l => l.SnapshotDate < DateTime.Now.Date)
                    .FirstOrDefault();

            if (lastSnapshot == null)
                return leagueTable;

            AddPreviousPositionsToRows(leagueTable, lastSnapshot);

            return leagueTable;
        }

        private void AddPreviousPositionsToRows(List<LeagueTableRow> leagueTable, LeagueTableSnapshot lastSnapshot)
        {
            foreach(var row in leagueTable)
            {
                var correspondingPlayerRow = lastSnapshot.Rows
                    .FirstOrDefault(r => r.PlayerFace == row.PlayerFace);

                var difference = Math.Abs(correspondingPlayerRow.Position - row.Position);

                row.PositionChange = (row.Position < correspondingPlayerRow.Position)
                    ? difference *= -1
                    : difference;
            }
        }
    }
}
