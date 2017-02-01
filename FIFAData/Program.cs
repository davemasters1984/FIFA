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
        
        static League _newLeague;

        static void Main(string[] args)
        {
            CreateDocumentStore();

            //InstallAllPlayers();

            //ImportTeams();

            SetParticipantNames();

            GenerateAssignmentsForParticipants();

            CreateLeagueFromAssignments();

            SaveLeague();

            OutputAssignmentsToConsole();

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
            var teamAssigner = new TeamAssigner(_documentStore);

            _assignments = teamAssigner.GetAssignments(_participantNames);
        }

        private static void CreateLeagueFromAssignments()
        {
            _newLeague = new League();

            _newLeague.Participants = new List<LeagueParticipant>();

            foreach(var assignment in _assignments)
            {
                _newLeague.Participants.Add(new LeagueParticipant
                {
                    ParticipantId = assignment.Player.Id,
                    TeamId = assignment.Team.Id
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
                Url = "http://localhost",
                DefaultDatabase = "FIFA",
                
            };

            _documentStore.Initialize();
        }
    }
}
