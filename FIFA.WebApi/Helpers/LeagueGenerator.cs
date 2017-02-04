using FIFA.Model;
using FIFA.Model.Assigners;
using FIFA.WebApi.Extensions;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FIFA.WebApi.Helpers
{
    public class LeagueGenerator
    {
        private IDocumentStore _documentStore;
        private IEnumerable<TeamAssignment> _assignments;
        private IEnumerable<string> _participantNames;
        private IEnumerable<Player> _players;
        private IEnumerable<Team> _teams;
        private IEnumerable<League> _previousLeagues;
        private IEnumerable<int> _possibleTeamRatings;
        private League _newLeague;

        public League Generate(IEnumerable<string> participantNames)
        {
            CreateDocumentStore();

            SetParticipantNames(participantNames);

            FetchRequiredData();

            GenerateAssignmentsForParticipants();

            CreateLeagueFromAssignments();

            SaveLeague();

            return _newLeague;
        }

        private void GenerateAssignmentsForParticipants()
        {
            var teamAssigner = new TeamAssigner(new TeamAssigner.TeamAssignerArgs
            {
                Players = _players,
                Teams = _teams,
                PossibleTeamRatings = _possibleTeamRatings,
                PreviousLeagues = _previousLeagues
            });

            _assignments = teamAssigner.GetAssignments(_participantNames);
        }

        private void SetParticipantNames(IEnumerable<string> participantNames)
        {
            _participantNames = participantNames;
        }

        private void CreateLeagueFromAssignments()
        {
            _newLeague = new League();
            _newLeague.Name = DateTime.Now.ToString("dd MMM yyyy");
            _newLeague.Participants = new List<Participant>();

            foreach (var assignment in _assignments)
            {
                _newLeague.Participants.Add(new Participant
                {
                    ParticipantId = assignment.Player.Id,
                    TeamId = assignment.Team.Id,
                    EligibleTeamRatings = assignment.EligibleTeamRatings
                });
            }

        }

        private void SaveLeague()
        {
            using (var session = _documentStore.OpenSession())
            {
                session.Store(_newLeague);

                session.SaveChanges();
            }
        }

        private  string GetCommaDeliminatedEligableRatings(IEnumerable<int> eligibleTeamRatings)
        {
            return string.Join(",",
                eligibleTeamRatings
                    .Select(r => r.ToString())
                    .ToArray());
        }

        private void InstallAllPlayers()
        {
            using (var session = _documentStore.OpenSession())
            {
                foreach (var participant in Player.AllPlayers)
                    session.Store(participant);

                session.SaveChanges();
            }
        }

        public void CreateDocumentStore()
        {
            _documentStore = new DocumentStore
            {
                ConnectionStringName = "RavenHQ",
                DefaultDatabase = "FIFA",
            };

            _documentStore.Initialize();
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
                    .Select(t => t.OverallRating)
                    .Distinct()
                    .ToList()
                    .OrderBy(r => r)
                    .ToList();
            }
        }
    }
}