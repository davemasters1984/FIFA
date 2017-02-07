using FIFA.CommandServices;
using FIFA.Infrastructure;
using FIFA.Model;
using FIFA.Model.Services;
using FIFAData.DataImport;
using Raven.Client;
using Raven.Client.Document;
using System;
using System.Collections.Generic;

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

            Console.WriteLine("Players & Teams installed successfully");

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

        private static void CreateLeague()
        {
            var leagueService = new LeagueCommandService(new RavenRepository(), new LeagueService(), new ResultService(new RavenRepository()));

            leagueService.CreateLeague(new FIFA.CommandServices.Interface.CreateLeagueCommand
            {
                ParticipantFaces = _participantNames
            });
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
    }
}
