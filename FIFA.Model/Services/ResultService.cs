using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.Model.Services
{
    public class ResultService
    {
        private IDocumentStore _documentStore;

        public ResultService(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public void PostResult(PostResultArgs args)
        {
            using (var session = _documentStore.OpenSession())
            {
                ValidateArgs(args, session);

                var league = session.Load<League>(args.LeagueId);
                var result = CreateResult(args);

                session.Store(result);

                league.PostResult(result);

                session.Store(league);
                session.SaveChanges();
            }
        }

        private void ValidateArgs(PostResultArgs args, IDocumentSession session)
        {
            var resultsForFixture = GetLeagueResultsForFixture(args, session);

            if (IsFixtureAlreadyBeenPlayedHomeAndAway(resultsForFixture))
                throw new Exception("This fixture has already been played twi fceor the home and away games.");

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

        private IEnumerable<Result> GetLeagueResultsForFixture(PostResultArgs args, IDocumentSession session)
        {
            var leagueResults = session.Query<Result>()
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

        protected string TranslateId<T>(int id)
        {
            return _documentStore.Conventions.FindFullDocumentKeyFromNonStringIdentifier(id, typeof(T), false);
        }
        protected string TranslateId<T>(string id)
        {
            return _documentStore.Conventions.FindFullDocumentKeyFromNonStringIdentifier(id, typeof(T), false);
        }
    }
}
