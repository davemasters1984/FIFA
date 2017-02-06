using FIFA.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FIFA.Model.Services
{
    public class ResultService
    {
        #region Private Fields

        private IRepository _repository;

        #endregion

        #region Constructor

        public ResultService(IRepository repository)
        {
            _repository = repository;
        }

        #endregion

        #region Public Methods

        public void PostResult(PostResultArgs args)
        {
            ValidateArgs(args);

            var league = _repository.Load<League>(args.LeagueId);
            var result = CreateResult(args);

            _repository.Store(result);

            league.PostResult(result);

            _repository.Store(league);
        }

        #endregion

        #region Private Methods

        private void ValidateArgs(PostResultArgs args)
        {
            var resultsForFixture = GetLeagueResultsForFixture(args);

            if (IsFixtureAlreadyBeenPlayedHomeAndAway(resultsForFixture))
                throw new Exception("This fixture has already been played twice for the home and away games.");

            if (DoesHomeAndAwayPlayersNeedSwapping(resultsForFixture, args))
                SwapHomeAndAwayPlayers(args);
        }

        private bool IsFixtureAlreadyBeenPlayedHomeAndAway(IEnumerable<Result> resultsForFixture)
        {
            return resultsForFixture.Count() == 2;
        }

        private bool DoesHomeAndAwayPlayersNeedSwapping(IEnumerable<Result> resultsForFixture, PostResultArgs args)
        {
            return resultsForFixture.Any(r => r.HomePlayerId == args.HomePlayerId && r.AwayPlayerId == args.AwayPlayerId);
        }

        private void SwapHomeAndAwayPlayers(PostResultArgs args)
        {
            string originalHomePlayer = args.HomePlayerId;
            int originalHomeGoals = args.HomePlayerGoals;
            string originalAwayPlayer = args.AwayPlayerId;
            int originalAwayGoals = args.AwayPlayerGoals;

            args.HomePlayerGoals = originalAwayGoals;
            args.HomePlayerId = originalAwayPlayer;
            args.AwayPlayerId = originalHomePlayer;
            args.AwayPlayerGoals = originalHomeGoals;
        }

        private IEnumerable<Result> GetLeagueResultsForFixture(PostResultArgs args)
        {
            var leagueResults = _repository.Query<Result>()
                .Where(r => r.LeagueId == args.LeagueId)
                .Where(r => r.HomePlayerId == args.HomePlayerId && r.AwayPlayerId == args.AwayPlayerId
                        || r.HomePlayerId == args.AwayPlayerId && r.AwayPlayerId == args.HomePlayerId)
                .ToList();

            return leagueResults;
        }

        private Result CreateResult(PostResultArgs args)
        {
            return new Result
            {
                AwayPlayerId = args.AwayPlayerId,
                HomePlayerId = args.HomePlayerId,
                LeagueId = args.LeagueId,
                AwayPlayerGoals = args.AwayPlayerGoals,
                HomePlayerGoals = args.HomePlayerGoals,
                Date = DateTime.Now
            };
        }

        #endregion
    }
}
