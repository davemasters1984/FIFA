using FIFA.CommandServices.Interface;
using FIFA.Infrastructure;
using FIFA.Model;
using FIFA.Model.Services;
using FIFA.QueryServices.Interface;
using FIFA.QueryServices.Interface.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FIFA.CommandServices
{
    public class LeagueCommandService : BaseCommandService, ILeagueCommandService
    {
        private ILeagueService _leagueService;
        private IResultService _resultService;
        private IRepository _repository;
        private ILeagueQueryService _leagueQueryService;

        public LeagueCommandService(IRepository repository, 
            ILeagueService leagueService, 
            IResultService resultService,
            ILeagueQueryService leagueQueryService)
        {
            _repository = repository;
            _leagueService = leagueService;
            _resultService = resultService;
            _leagueQueryService = leagueQueryService;
        }

        public void CreateLeague(CreateLeagueCommand command)
        {
            var helper = new CreateLeagueHelper();

            var args = helper.CreateLeagueArgs(command.ParticipantFaces);

            var leagueService = new LeagueService();

            var league = leagueService.CreateNewLeague(args);

            using (var unitOfWork = UnitOfWorkFactory.CreateUnitOfWork())
            {
                _repository.Store(league);
            }
        }

        public void PostResult(PostResultCommand command)
        {
            using (var unitOfWork = UnitOfWorkFactory.CreateUnitOfWork())
            {
                _resultService.PostResult(command.AsArgs());
            }

            TakeSnapshot(new TakeSnapshotCommand());
        }

        public void TakeSnapshot(TakeSnapshotCommand command)
        {
            var currentLeagueId = _leagueQueryService.GetCurrentLeagueId();
            var currentLeague = _leagueQueryService.GetLeagueTable(currentLeagueId);
            var currentDate = DateTime.Now.Date;

            using (var unitOfWork = UnitOfWorkFactory.CreateUnitOfWork())
            {
                var snapshot = _repository.Query<LeagueTableSnapshot>()
                    .Where(s => s.SnapshotDate == currentDate)
                    .Where(s => s.LeagueId == currentLeagueId)
                    .FirstOrDefault();

                if (snapshot == null)
                    snapshot = new LeagueTableSnapshot();

                snapshot.Rows = MapSnapshotRows(currentLeague);
                snapshot.SnapshotDate = currentDate;
                snapshot.LeagueId = currentLeagueId;

                _repository.Store(snapshot);
            }
        }

        private List<SnapshotRow> MapSnapshotRows(IEnumerable<LeagueTableRow> rows)
        {
            var snapshotRows = new List<SnapshotRow>();

            foreach (var row in rows)
                snapshotRows.Add(MapToSnapshotRow(row));

            return snapshotRows;
        }

        private SnapshotRow MapToSnapshotRow(LeagueTableRow row)
        {
            return new SnapshotRow
            {
                Position = row.Position,
                TeamId = row.TeamId,
                GamesDrawn = row.GamesDrawn,
                GamesLost = row.GamesLost,
                GamesPlayed = row.GamesPlayed,
                GamesWon = row.GamesWon,
                GoalsAgainst = row.GoalsAgainst,
                GoalsFor = row.GoalsFor,
                PlayerFace = row.PlayerFace,
                PlayerName = row.PlayerName,
                PlayerId = row.PlayerId,
                Points = row.Points,
                TeamBadge = row.TeamBadge,
                TeamName = row.TeamName,
                TeamRating = row.TeamRating
            };
        }
    }
}
