﻿using FIFA.Model;
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
        private IPlayerQueryService _playerQueryService;

        public LeagueQueryService(IDocumentStore documentStore, IPlayerQueryService playerQueryService)
        {
            _documentStore = documentStore;
            _playerQueryService = playerQueryService;
        }

        #region Public Methods

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

        public IEnumerable<LeagueTableRow> GetLeagueTableWaitForIndex(string leagueId)
        {
            using (var session = _documentStore.OpenSession())
                return GetLeagueTable(session, leagueId, true);
        }

        public IEnumerable<FormTableRow> GetFormTable(string leagueId)
        {
            using (var session = _documentStore.OpenSession())
            {
                var form = session.Query<FormTableRow, FormTableIndex>()
                    .Where(l => l.LeagueId == leagueId)
                    .OrderByDescending(l => l.TotalPoints)
                    .ToList();

                return form;
            }
        }

        public IEnumerable<PlayerPositionHistory> GetPostionHistoryForPlayers(string leagueId, IEnumerable<string> playerIds)
        {
            var history = new List<PlayerPositionHistory>();

            using (var session = _documentStore.OpenSession())
            {
                var snapshots = session.Query<LeagueTableSnapshot>()
                    .Where(s => s.LeagueId == leagueId)
                    .ToList();

                if (!snapshots.Any())
                    return Enumerable.Empty<PlayerPositionHistory>();

                foreach (var playerId in playerIds)
                    history.Add(GetPlayerPositionHistory(snapshots, playerId, session));

                return history;
            }
        }

        public PlayerPositionHistoryComparison GetPlayerPositionHistoryComparisonForCurrentLeague(string playerOneId, string playerTwoId)
        {
            using (var session = _documentStore.OpenSession())
            {
                var leagueId = GetCurrentLeagueId(session);
                var playerOne = session.Load<Player>(playerOneId);
                var playerTwo = session.Load<Player>(playerTwoId);

                var playerHistory = new PlayerPositionHistoryComparison
                {
                    PlayerOneId = playerOneId,
                    PlayerTwoId = playerTwoId,
                    PlayerOneName = playerOne.Name,
                    PlayerTwoName = playerTwo.Name
                };

                var snapshots = session.Query<LeagueTableSnapshot>()
                    .Where(s => s.LeagueId == leagueId)
                    .ToList();

                playerHistory.PlayerOnePositionHistory = snapshots
                    .Select(r => new PlayerPosition
                    {
                        Date = r.SnapshotDate,
                        Position = r.Rows.Where(x => x.PlayerId == playerOneId)
                                .Select(x => x.Position)
                                .FirstOrDefault()
                    })
                    .ToList();

                playerHistory.PlayerTwoPositionHistory = snapshots
                    .Select(r => new PlayerPosition
                    {
                        Date = r.SnapshotDate,
                        Position = r.Rows.Where(x => x.PlayerId == playerTwoId)
                                .Select(x => x.Position)
                                .FirstOrDefault()
                    })
                    .ToList();

                return playerHistory;
            }
        }

        #endregion

        #region Private Methods 

        private PlayerPositionHistory GetPlayerPositionHistory(IEnumerable<LeagueTableSnapshot> snapshots, string playerId, IDocumentSession session)
        {
            var player = session.Load<Player>(playerId);

            return new PlayerPositionHistory
            {
                PlayerId = playerId,
                PlayerFace = player.Face,
                PlayerName = player.Name,
                History = snapshots.Select(r => new PlayerPosition
                {
                    Date = r.SnapshotDate,
                    Position = r.Rows.Where(x => x.PlayerId == playerId)
                        .Select(x => x.Position)
                        .FirstOrDefault()
                })
                .ToList()
            };
        }

        private string GetCurrentLeagueId(IDocumentSession session)
        {
            var leagueId = session.Query<League>()
                .OrderByDescending(l => l.CreatedDate)
                .Select(l => l.Id)
                .FirstOrDefault();

            return leagueId;
        }

        private IEnumerable<LeagueTableRow> GetLeagueTable(IDocumentSession session, string leagueId, bool waitForFreshIndex)
        {
            var leagueTableQuery
                = session.Query<LeagueTableRow, LeagueTableIndex>();

            if (waitForFreshIndex)
                leagueTableQuery = leagueTableQuery.Customize(c => c.WaitForNonStaleResultsAsOfNow());

            var orderedleagueTable = leagueTableQuery
                    .Where(l => l.LeagueId == leagueId)
                    .OrderByDescending(l => l.Points)
                    .ThenByDescending(l => l.GoalDifference)
                    .ThenByDescending(l => l.GoalsFor)
                    .ToList();

            var lastSnapshot
                = session.Query<LeagueTableSnapshot>()
                    .Where(l => l.LeagueId == leagueId)
                    .Where(l => l.SnapshotDate < DateTime.Now.Date)
                    .OrderByDescending(l => l.SnapshotDate)
                    .FirstOrDefault();

            if (lastSnapshot == null)
                return orderedleagueTable;

            AddPreviousPositionsToRows(orderedleagueTable, lastSnapshot);

            return orderedleagueTable;
        }

        private IEnumerable<LeagueTableRow> GetLeagueTable(IDocumentSession session, string leagueId)
        {
            return GetLeagueTable(session, leagueId, false);
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

        public CurrentLeagueAndPlayerIds GetCurrentLeagueAndPlayerIds(string playerFaceOne, string playerFaceTwo)
        {
            using (var session = _documentStore.OpenSession())
            {
                var data = new CurrentLeagueAndPlayerIds
                {
                    LeagueId = GetCurrentLeagueId(session),
                    PlayerOneId = session.Query<Player>()
                        .Where(p => p.Face == playerFaceOne)
                        .Select(p => p.Id)
                        .FirstOrDefault(),
                    PlayerTwoId = session.Query<Player>()
                        .Where(p => p.Face == playerFaceTwo)
                        .Select(p => p.Id)
                        .FirstOrDefault(),
                };

                return data;
            }
        }

        public IEnumerable<ResultSummary> GetHeadToHeadResults(string leagueId, string faceOne, string faceTwo)
        {
            using (var session = _documentStore.OpenSession())
            {
                var results = session.Query<ResultSummary, ResultsIndex>()
                    .Where(l => l.LeagueId == leagueId)
                    .Where(l => (l.HomePlayerFace == faceOne && l.AwayPlayerFace == faceTwo) || (l.HomePlayerFace == faceTwo && l.AwayPlayerFace == faceOne))
                    .ToList();

                return results;
            }
        }

        #endregion
    }
}
