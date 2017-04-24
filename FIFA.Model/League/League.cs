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

        public List<Fixture> Fixtures { get; set; }

        public string Name { get; set; }

        public bool IsComplete
        {
            get
            {
                if (Fixtures == null)
                    return false;

                return Fixtures.Count() == Fixtures.Where(f => f.Result != null).Count();
            }
        }

        public League()
        {

        }

        public League(DateTime createdDate, 
            List<Participant> participants, 
            List<Fixture> fixtures)
        {
            Participants = participants;
            Fixtures = fixtures;
            CreatedDate = createdDate;
        }

        public void PostResult(PostResultArgs args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            ValidateArgs(args);

            var fixture = GetFixtureForResult(args);

            fixture.Result = CreateResult(args);

            UpdateParticipantsForPostResult(fixture);
        }

        private Fixture GetFixtureForResult(PostResultArgs args)
        {
            var fixture = Fixtures.FirstOrDefault(f => f.HomePlayerId == args.HomePlayerId && f.AwayPlayerId == args.AwayPlayerId);

            return fixture;
        }

        private void UpdateParticipantsForPostResult(Fixture fixture)
        {
            UpdateHomeParticipantForPostedResult(fixture);
            UpdateAwayParticipantForPostedResult(fixture);
            AssignPositions();
        }

        private void UpdateHomeParticipantForPostedResult(Fixture fixture)
        {
            var participant = FindParticipant(fixture.HomePlayerId);

            participant.PostResultAsHomePlayer(fixture.Result);
        }

        private void UpdateAwayParticipantForPostedResult(Fixture fixture)
        {
            var participant = FindParticipant(fixture.AwayPlayerId);

            participant.PostResultAsAwayPlayer(fixture.Result);
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
            var fixturesForResult = GetFixturesForPlayers(args);

            if (IsFixtureAlreadyBeenPlayedHomeAndAway(fixturesForResult))
                throw new Exception("This fixture has already been played twice for the home and away games.");

            if (DoesHomeAndAwayPlayersNeedSwapping(fixturesForResult, args))
                SwapHomeAndAwayPlayers(args);
        }

        private bool IsFixtureAlreadyBeenPlayedHomeAndAway(IEnumerable<Fixture> alreadyPlayedFixtures)
        {
            return alreadyPlayedFixtures.Count() == 2;
        }

        private bool DoesHomeAndAwayPlayersNeedSwapping(IEnumerable<Fixture> resultsForFixture, PostResultArgs args)
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

        private IEnumerable<Fixture> GetFixturesForPlayers(PostResultArgs args)
        {
            if (this.Fixtures == null)
                return Enumerable.Empty<Fixture>();

            var fixturesForPlayers = Fixtures
                .Where(f => f.HomePlayerId == args.HomePlayerId && f.AwayPlayerId == args.AwayPlayerId
                        || f.HomePlayerId == args.AwayPlayerId && f.AwayPlayerId == args.HomePlayerId)
                .Where(f => f.Result != null)
                .ToList();

            return fixturesForPlayers;
        }

        private Result CreateResult(PostResultArgs args)
        {
            return new Result
            {
                AwayPlayerGoals = args.AwayPlayerGoals,
                HomePlayerGoals = args.HomePlayerGoals,
                Date = DateTime.Now
            };
        }

        private void AssignPositions()
        {
            AssignPositionsFromPoints();
        }

        private void AssignPositionsFromPoints()
        {
            var participantsOrderedByPoints
                = Participants
                    .OrderByDescending(p => p.Points)
                    .ThenByDescending(p => (p.GoalsFor - p.GoalsAgainst))
                    .ThenByDescending(p => p.GoalsFor);

            int position = 1;

            foreach (var participant in participantsOrderedByPoints)
                participant.Position = position++;
        }
    }
}