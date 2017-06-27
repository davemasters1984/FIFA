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

        public string GetCurrentLeagueIdForPlayer(string face)
        {
            var playerId = _playerQueryService.ResolvePlayerId(face);

            using (var session = _documentStore.OpenSession())
            {
                var leagueId = session.Query<League>()
                    .Where(l => !l.IsComplete)
                    .Where(l => l.Participants.Any(p => p.PlayerId == playerId))
                    .OrderByDescending(l => l.CreatedDate)
                    .Select(l => l.Id)
                    .FirstOrDefault();

                return leagueId;
            }
        }

        public string GetCurrentLeagueIdFromLeagueName(string leagueName)
        {
            var resolvedName = LeagueNameHelper.ResolveLeagueName(leagueName);

            using (var session = _documentStore.OpenSession())
            {
                var leagueId = session.Query<League>()
                    .Where(l => l.Name == resolvedName)
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

        public IEnumerable<FixtureSummary> GetFixturesForLeagueId(string leagueId)
        {
            using (var session = _documentStore.OpenSession())
            {
                var fixtures = session.Query<FixtureSummary, FixturesIndex>()
                    .Where(l => l.LeagueId == leagueId)
                    .ToList();

                return fixtures;
            }
        }

        public IEnumerable<LeagueTableRow> GetLeagueTable(string leagueId)
        {
            using (var session = _documentStore.OpenSession())
                return GetLeagueTable(session, leagueId);
        }

        public IEnumerable<LeagueTableRow> GetLeagueTableWithPositionHistory(string leagueId)
        {
            using (var session = _documentStore.OpenSession())
                return GetLeagueTable(session, leagueId, false, true);
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

        public IEnumerable<FormTableRow> GetFormTable(string leagueId, int games)
        {
            using (var session = _documentStore.OpenSession())
            {
                var league = session.Load<League>(leagueId);
                var teams = session.Query<Team>().ToList();
                var players = session.Query<Player>().ToList();

                var results = 
                    (from p in league.Participants
                    select new FormTableRow
                    {
                        LeagueId = league.Id,
                        PlayerFace = players.Where(pl => pl.Id == p.PlayerId).Select(pl => pl.Face).FirstOrDefault(),
                        PlayerId = p.PlayerId,
                        TeamBadge = teams.Where(t => t.Id == p.TeamId).Select(t => t.Badge).FirstOrDefault(),
                        TeamId = p.TeamId,
                        Results = league.Fixtures
                                    .Where(f => f.Result != null)
                                    .Where(f => f.HomePlayerId == p.PlayerId || f.AwayPlayerId == p.PlayerId)
                                    .OrderByDescending(f => f.Result.Date)
                                    .Take(games)
                                    .Select(f => new Res
                                    {
                                        HomePlayerId = f.HomePlayerId,
                                        AwayPlayerId = f.AwayPlayerId,
                                        HomePoints = f.Result.HomePoints,
                                        AwayPoints = f.Result.AwayPoints
                                    }),
                        TotalPoints = 1,
                    })
                    .ToList();

                var form = from r in results
                            group r by new
                            {
                                LeagueId = r.LeagueId,
                                PlayerId = r.PlayerId,
                                PlayerFace = r.PlayerFace,
                                TeamBadge = r.TeamBadge,
                                TeamId = r.TeamId,
                                Results = r.Results,
                            }
                            into g
                            select new FormTableRow
                            {
                                LeagueId = g.Key.LeagueId,
                                PlayerId = g.Key.PlayerId,
                                PlayerFace = g.Key.PlayerFace,
                                TeamBadge = g.Key.TeamBadge,
                                TeamId = g.Key.TeamId,
                                Results = g.Key.Results,
                                TotalPoints = g.Key.Results.Where(r => r.HomePlayerId == g.Key.PlayerId).Sum(r => r.HomePoints) +
                                              g.Key.Results.Where(r => r.AwayPlayerId == g.Key.PlayerId).Sum(r => r.AwayPoints)
                            };

                return form
                    .OrderByDescending(f => f.TotalPoints)
                    .ToList();
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
                    .Select(r => new PlayerPositionAtDate
                    {
                        Date = r.SnapshotDate,
                        Position = r.Rows.Where(x => x.PlayerId == playerOneId)
                                .Select(x => x.Position)
                                .FirstOrDefault()
                    })
                    .ToList();

                playerHistory.PlayerTwoPositionHistory = snapshots
                    .Select(r => new PlayerPositionAtDate
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

        private IEnumerable<LeagueTableRecentResult> GetRecentResultsForPlayer(League league, List<LeagueTableRow> leagueRows, string playerId, IDocumentSession session)
        {
            var recentResults
                = league.Fixtures
                    .Where(f => f.Result != null)
                    .Where(f => f.HomePlayerId == playerId || f.AwayPlayerId == playerId)
                    .OrderByDescending(f => f.Result.Date)
                    .Take(4)
                    .Select(f => new LeagueTableRecentResult
                    {
                        ResultDate = f.Result.Date,
                        OpponentGoals = (f.HomePlayerId == playerId) ? f.Result.AwayPlayerGoals : f.Result.HomePlayerGoals,
                        PlayerGoals = (f.HomePlayerId == playerId) ? f.Result.HomePlayerGoals : f.Result.AwayPlayerGoals,
                        OpponentPlayerName = (f.HomePlayerId == playerId)
                            ? leagueRows.Where(p => p.PlayerId == f.AwayPlayerId).Select(p => p.PlayerName).FirstOrDefault()
                            : leagueRows.Where(p => p.PlayerId == f.HomePlayerId).Select(p => p.PlayerName).FirstOrDefault(),
                        OpponentTeamName = (f.HomePlayerId == playerId)
                            ? leagueRows.Where(p => p.PlayerId == f.AwayPlayerId).Select(p => p.TeamName).FirstOrDefault()
                            : leagueRows.Where(p => p.PlayerId == f.HomePlayerId).Select(p => p.TeamName).FirstOrDefault(),
                        OpponentPlayerFace = (f.HomePlayerId == playerId)
                            ? leagueRows.Where(p => p.PlayerId == f.AwayPlayerId).Select(p => p.PlayerFace).FirstOrDefault()
                            : leagueRows.Where(p => p.PlayerId == f.HomePlayerId).Select(p => p.PlayerFace).FirstOrDefault(),
                    })
                    .ToList();

            return recentResults;
        }

        private PlayerPositionHistory GetPlayerPositionHistory(IEnumerable<LeagueTableSnapshot> snapshots, string playerId, IDocumentSession session)
        {
            var player = session.Load<Player>(playerId);

            return new PlayerPositionHistory
            {
                PlayerId = playerId,
                PlayerFace = player.Face,
                PlayerName = player.Name,
                History = snapshots.Select(r => new PlayerPositionAtDate
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

        private IEnumerable<LeagueTableRow> GetLeagueTable(IDocumentSession session, string leagueId, bool waitForFreshIndex, bool includePositionHistory = false)
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

            AddPreviousPositionsToRows(orderedleagueTable, lastSnapshot);

            if (includePositionHistory)
                AddPositionHistoryToRows(orderedleagueTable, leagueId, session);

            return orderedleagueTable;
        }


        private void AddPositionHistoryToRows(List<LeagueTableRow> orderedleagueTable, string leagueId, IDocumentSession session)
        {
            var league = session.Load<League>(leagueId);

            foreach (var row in orderedleagueTable)
            {
                var history = league.Participants
                    .First(p => p.PlayerId == row.PlayerId)
                    .PositionHistory;

                row.PositionHistory = GetPositionHistoryForPlayer(row, league, orderedleagueTable);
            }
                
        }

        private IEnumerable<PlayerPositionAtGamesPlayed> GetPositionHistoryForPlayer(LeagueTableRow row, 
            League league, 
            List<LeagueTableRow> orderedleagueTable)
        {
            var historyAtGamesPlayed = new List<PlayerPositionAtGamesPlayed>();

            var history = league.Participants
                .First(p => p.PlayerId == row.PlayerId)
                .PositionHistory;

            if (history == null)
                return historyAtGamesPlayed;

            foreach (var historyItem in history)
            {
                var positionAtGamesPlayed = new PlayerPositionAtGamesPlayed
                {
                    Position = historyItem.Position,
                    GamesPlayed = historyItem.GamesPlayed
                };



                historyAtGamesPlayed.Add(positionAtGamesPlayed);

                var fixture = GetFixture(league, historyItem.HomePlayerId, historyItem.AwayPlayerId);

                if (fixture == null)
                    continue;

                positionAtGamesPlayed.HomePlayerId = fixture.HomePlayerId;               
                positionAtGamesPlayed.AwayPlayerId = fixture.AwayPlayerId;

                if (fixture.Result == null)
                    continue;

                if (row.PlayerId == historyItem.HomePlayerId)
                    positionAtGamesPlayed.IsWin = (fixture.Result.HomePlayerGoals > fixture.Result.AwayPlayerGoals);
                else
                    positionAtGamesPlayed.IsWin = (fixture.Result.AwayPlayerGoals > fixture.Result.HomePlayerGoals);

                positionAtGamesPlayed.IsDraw = (row.PlayerId == historyItem.HomePlayerId && fixture.Result.HomePlayerGoals == fixture.Result.AwayPlayerGoals);

                if (row.PlayerId == historyItem.HomePlayerId)
                    positionAtGamesPlayed.IsLoss = (fixture.Result.AwayPlayerGoals > fixture.Result.HomePlayerGoals);
                else
                    positionAtGamesPlayed.IsLoss = (fixture.Result.HomePlayerGoals > fixture.Result.AwayPlayerGoals);

                positionAtGamesPlayed.ResultDate = fixture.Result.Date;

                positionAtGamesPlayed.HomeGoals = fixture.Result.HomePlayerGoals;
                positionAtGamesPlayed.HomePlayerFace = GetPlayerFaceFromLeagueRows(fixture.HomePlayerId, orderedleagueTable);
                positionAtGamesPlayed.HomePlayerName = GetPlayerNameFromLeagueRows(fixture.HomePlayerId, orderedleagueTable);
                positionAtGamesPlayed.HomePlayerTeamName = GetPlayersTeamNameFromLeagueRows(fixture.HomePlayerId, orderedleagueTable);
                positionAtGamesPlayed.HomePlayerTeamBadge = GetPlayersTeamBadgeFaceFromLeagueRows(fixture.HomePlayerId, orderedleagueTable);

                positionAtGamesPlayed.AwayGoals = fixture.Result.AwayPlayerGoals;
                positionAtGamesPlayed.AwayPlayerFace = GetPlayerFaceFromLeagueRows(fixture.AwayPlayerId, orderedleagueTable);
                positionAtGamesPlayed.AwayPlayerName = GetPlayerNameFromLeagueRows(fixture.AwayPlayerId, orderedleagueTable);
                positionAtGamesPlayed.AwayPlayerTeamName = GetPlayersTeamNameFromLeagueRows(fixture.AwayPlayerId, orderedleagueTable);
                positionAtGamesPlayed.AwayPlayerTeamBadge = GetPlayersTeamBadgeFaceFromLeagueRows(fixture.AwayPlayerId, orderedleagueTable);
            }

            return historyAtGamesPlayed;
        }

        private string GetPlayerNameFromLeagueRows(string playerId, List<LeagueTableRow> leagueRows)
        {
            return leagueRows
                .Where(r => r.PlayerId == playerId)
                .Select(l => l.PlayerName)
                .FirstOrDefault();
        }

        private string GetPlayerFaceFromLeagueRows(string playerId, List<LeagueTableRow> leagueRows)
        {
            return leagueRows
                .Where(r => r.PlayerId == playerId)
                .Select(l => l.PlayerFace)
                .FirstOrDefault();
        }

        private string GetPlayersTeamNameFromLeagueRows(string playerId, List<LeagueTableRow> leagueRows)
        {
            return leagueRows
                .Where(r => r.PlayerId == playerId)
                .Select(l => l.TeamName)
                .FirstOrDefault();
        }

        private string GetPlayersTeamBadgeFaceFromLeagueRows(string playerId, List<LeagueTableRow> leagueRows)
        {
            return leagueRows
                .Where(r => r.PlayerId == playerId)
                .Select(l => l.TeamBadge)
                .FirstOrDefault();
        }

        private Fixture GetFixture(League league, string homePlayerId, string awayPlayerId)
        {
            return league.Fixtures.FirstOrDefault(f => f.HomePlayerId == homePlayerId && f.AwayPlayerId == awayPlayerId);
        }

        private IEnumerable<LeagueTableRow> GetLeagueTable(IDocumentSession session, string leagueId)
        {
            return GetLeagueTable(session, leagueId, false);
        }


        private void AddPreviousPositionsToRows(List<LeagueTableRow> leagueTable, LeagueTableSnapshot lastSnapshot)
        {
            if (lastSnapshot == null)
                return;

            foreach(var row in leagueTable)
            {
                var correspondingPlayerRow = lastSnapshot.Rows
                    .FirstOrDefault(r => r.PlayerFace == row.PlayerFace);

                if (correspondingPlayerRow == null)
                    continue;

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

        public LeagueTable GetLeagueTableWithHeader(string leagueId)
        {
            using (var session = _documentStore.OpenSession())
            {
                var league = session.Load<League>(leagueId);

                var leagueTable = new LeagueTable
                {
                    LeagueName = league.Name,
                    IsBottomLeague = league.IsBottomLeague,
                    IsTopLeague = league.IsTopLeague,
                };

                leagueTable.Rows = GetLeagueTable(session, leagueId);

                return leagueTable;
            }
        }

        #endregion
    }
}
