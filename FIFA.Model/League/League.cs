using FIFA.Model.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FIFA.Model
{
    public class League
    {
        public string Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public List<Participant> Participants { get; set; }

        public List<Result> Results { get; set; }

        public List<Fixture> Fixtures { get; set; }

        public void PostResult(PostResultArgs args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            ValidateArgs(args);

            var result = CreateResult(args);

            UpdateParticipantsForPostResult(result);

            if (Results == null)
                Results = new List<Result>();

            Results.Add(result);
        }

        private void UpdateParticipantsForPostResult(Result result)
        {
            UpdateHomeParticipantForPostedResult(result);
            UpdateAwayParticipantForPostedResult(result);
        }

        private void UpdateHomeParticipantForPostedResult(Result result)
        {
            var participant = FindParticipant(result.HomePlayerId);

            participant.PostResultAsHomePlayer(result);
        }

        private void UpdateAwayParticipantForPostedResult(Result result)
        {
            var participant = FindParticipant(result.AwayPlayerId);

            participant.PostResultAsAwayPlayer(result);
        }

        private Participant FindParticipant(string playerId)
        {
            if (Participants == null)
                throw new Exception("This league has no participants");

            var participant = Participants.FirstOrDefault(p => p.PlayerId == playerId);

            if (participant == null)
                throw new Exception(string.Format("This league has no participant with a player Id of '{0}'", playerId));

            return participant;
        }

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
            if (this.Results == null)
                return Enumerable.Empty<Result>();

            var leagueResults = this.Results
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
    }
}