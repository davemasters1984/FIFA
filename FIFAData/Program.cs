using FIFA.Model;
using FIFA.Model.Assigners;
using FIFAData.DataImport;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FIFAData
{
    class Program
    {
        static IDocumentStore _documentStore;
        static IEnumerable<TeamAssignment> _assignments;
        static IEnumerable<string> _participantNames;
        static IEnumerable<Player> _players;
        static IEnumerable<Team> _teams;
        static IEnumerable<League> _previousLeagues;
        static IEnumerable<int> _possibleTeamRatings;
        static League _newLeague;

        static void Main(string[] args)
        {
            CreateDocumentStore();

            InstallAllPlayers();

            ImportTeams();

            //SetParticipantNames();

            //FetchRequiredData();

            //GenerateAssignmentsForParticipants();

            //CreateLeagueFromAssignments();

            //SaveLeague();

            //OutputAssignmentsToConsole();

            Console.Read();
        }

        private static void SetParticipantNames()
        {
            _participantNames = new string[] 
            {
                ":neil:",
                ":daveb:",
                ":mattw:",
                ":tristan:",
                ":dom:",
                ":matt:",
                ":liam:",
                ":james:",
                ":louie:",
                ":dave:",
                ":craig:",
                ":ash:",
                ":jakub:",
                ":mogg:",
                ":luke:"
            };
        }

        private static void GenerateAssignmentsForParticipants()
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

        private static void CreateLeagueFromAssignments()
        {
            _newLeague = new League();

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

        private static void SaveLeague()
        {
            using (var session = _documentStore.OpenSession())
            {
                session.Store(_newLeague);

                session.SaveChanges();
            }
        }        

        private static void OutputAssignmentsToConsole()
        {
            OutputHanicappedAssignmentsToConsole();

            OutputFourStarAssignmentsToConsole();
        }

        private static void OutputHanicappedAssignmentsToConsole()
        {
            Console.WriteLine("Handicaped Team Assignments:");
            Console.WriteLine("______________________________________________________________________________");

            foreach (var assignment in _assignments.Where(a => !a.Player.IsNew))
            {
                string ratings = GetCommaDeliminatedEligableRatings(assignment.EligibleTeamRatings);

                Console.WriteLine($"{assignment.Player.Name} - {ratings} - {assignment.Team.TeamName} ({assignment.Team.OverallRating}) ");
            }
        }

        private static void OutputFourStarAssignmentsToConsole()
        {
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("New Players 4 Star Team Assignments:");
            Console.WriteLine("______________________________________________________________________________");

            foreach (var assignment in _assignments.Where(a => a.Player.IsNew))
            {
                Console.WriteLine($"{assignment.Player.Name} - {assignment.Team.TeamName} ({assignment.Team.OverallRating}) ");
            }
        }

        private static string GetCommaDeliminatedEligableRatings(IEnumerable<int> eligibleTeamRatings)
        {
            return string.Join(",",
                eligibleTeamRatings
                    .Select(r => r.ToString())
                    .ToArray());
        }

        private static void InstallAllPlayers()
        {
            using (var session = _documentStore.OpenSession())
            {
                foreach (var participant in Player.AllPlayers)
                    session.Store(participant);

                session.SaveChanges();
            }
        }

        private static void ImportTeams()
        {
            FIFATeamImporter.Import(_documentStore,
                @"C:\Git\FIFA\FIFAData\DataImport\FIFA.csv");
        }

        public static void CreateDocumentStore()
        {
            _documentStore = new DocumentStore
            {
                ConnectionStringName = "RavenHQ",
                DefaultDatabase = "FIFA",
            };

            _documentStore.Initialize();
        }

        private static void FetchRequiredData()
        {
            FetchTeams();

            FetchPreviousLeagues();

            FetchPossibleTeamRatings();

            FetchPlayersMatchingParticipantNames();
        }

        private static void FetchTeams()
        {
            using (var session = _documentStore.OpenSession())
            {
                session.Advanced.MaxNumberOfRequestsPerSession = 1000;
                _teams = session.GetAll<Team>()
                    .ToList();
            }
        }

        private static void FetchPreviousLeagues()
        {
            using (var session = _documentStore.OpenSession())
                _previousLeagues = session.Query<League>().ToList();
        }

        private static void FetchPlayersMatchingParticipantNames()
        {
            using (var session = _documentStore.OpenSession())
            {
                _players = session.Query<Player>()
                    .Where(p => p.Face.In(_participantNames))
                    .ToList();
            }
        }

        private static void FetchPossibleTeamRatings()
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
