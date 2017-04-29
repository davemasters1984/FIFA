using FIFA.Infrastructure;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Linq;
using System.Collections.Generic;
using System.Linq;

namespace FIFA.Model.Services
{
    public class CreateLeagueHelper
    {
        private IDocumentStore _documentStore;
        private IEnumerable<string> _participantNames;
        private IEnumerable<Player> _players;
        private IEnumerable<Team> _teams;
        private IEnumerable<League> _previousLeagues;
        private IEnumerable<int> _possibleTeamRatings;
        private decimal _minimumTeamRating;
        private decimal _maximumTeamRating;

        public CreateLeagueHelper(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public CreateLeagueArgs CreateLeagueArgs(IEnumerable<string> participantNames, 
            string leagueName, 
            decimal minimumTeamRating, 
            decimal maximumTeamRating)
        {
            _minimumTeamRating = minimumTeamRating;
            _maximumTeamRating = maximumTeamRating;

            SetParticipantNames(participantNames);

            FetchRequiredData();

            return new CreateLeagueArgs
            {
                Players = _players,
                PossibleTeamRatings = _possibleTeamRatings,
                PreviousLeagues = _previousLeagues,
                Teams = _teams,
                Name = leagueName,
            };
        }

        private void SetParticipantNames(IEnumerable<string> participantNames)
        {
            _participantNames = participantNames;
        }

        private  string GetCommaDeliminatedEligableRatings(IEnumerable<int> eligibleTeamRatings)
        {
            return string.Join(",",
                eligibleTeamRatings
                    .Select(r => r.ToString())
                    .ToArray());
        }

        private void FetchRequiredData()
        {
            FetchTeams();

            FetchPreviousLeagues();

            FetchPossibleTeamRatings();

            FetchPlayersMatchingParticipantNames();
        }

        private void FetchTeams()
        {
            using (var session = _documentStore.OpenSession())
            {
                session.Advanced.MaxNumberOfRequestsPerSession = 1000;
                _teams = session.GetAll<Team>()
                    .ToList();
            }
        }

        private void FetchPreviousLeagues()
        {
            using (var session = _documentStore.OpenSession())
                _previousLeagues = session.Query<League>().ToList();
        }

        private void FetchPlayersMatchingParticipantNames()
        {
            using (var session = _documentStore.OpenSession())
            {
                _players = session.Query<Player>()
                    .Where(p => p.Face.In(_participantNames))
                    .ToList();
            }
        }

        private void FetchPossibleTeamRatings()
        {
            using (var session = _documentStore.OpenSession())
            {
                _possibleTeamRatings = session.Query<Team>()
                    .Where(tr => tr.OverallRating != 0)
                    .Where(tr => tr.OverallRating >= _minimumTeamRating)
                    .Where(tr => tr.OverallRating <= _maximumTeamRating)
                    .Select(t => t.OverallRating)
                    .Distinct()
                    .ToList()
                    .OrderBy(r => r)
                    .ToList();
            }
        }
    }
}