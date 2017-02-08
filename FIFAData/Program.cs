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
        static IEnumerable<string> _participantNames;

        static void Main(string[] args)
        {
            CreateDocumentStore();

            CreatePreviousLeague();

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

        public static void CreatePreviousLeague()
        {
            var league = new League();
            league.Participants = new List<Participant>();

            league.CreatedDate = new DateTime(2016, 11, 01);

            league.Participants.Add(new Participant
            {
                PlayerId = "players/65",//dave
                TeamId = "teams/1386",
                Points = 31,
                GamesPlayed = 28,
                GoalsAgainst = 1,
                GoalsFor = 1
            });

            league.Participants.Add(new Participant
            {
                PlayerId = "players/66", //dom
                TeamId = "teams/1984",
                Points = 39,
                GamesPlayed = 28,
                GoalsAgainst = 1,
                GoalsFor = 1
            });

            league.Participants.Add(new Participant
            {
                PlayerId = "players/67", //matt n
                TeamId = "teams/1782",
                Points = 41,
                GamesPlayed = 28,
                GoalsAgainst = 1,
                GoalsFor = 1
            });

            league.Participants.Add(new Participant
            {
                PlayerId = "players/68",//liam
                TeamId = "teams/2001",
                Points = 29,
                GamesPlayed = 28,
                GoalsAgainst = 1,
                GoalsFor = 1
            });

            league.Participants.Add(new Participant
            {
                PlayerId = "players/69",//james c
                TeamId = "teams/1422",
                Points = 36,
                GamesPlayed = 28,
                GoalsAgainst = 1,
                GoalsFor = 1
            });

            league.Participants.Add(new Participant
            {
                PlayerId = "players/70",//louie
                TeamId = "teams/1553",
                Points = 39,
                GamesPlayed = 28,
                GoalsAgainst = 1,
                GoalsFor = 1
            });

            league.Participants.Add(new Participant
            {
                PlayerId = "players/71", //matt w
                TeamId = "teams/1998",
                Points = 47,
                GamesPlayed = 28,
                GoalsAgainst = 1,
                GoalsFor = 1
            });

            league.Participants.Add(new Participant
            {
                PlayerId = "players/72",//tristan
                TeamId = "teams/1527",
                Points = 45,
                GamesPlayed = 28,
                GoalsAgainst = 1,
                GoalsFor = 1
            });

            league.Participants.Add(new Participant
            {
                PlayerId = "players/73",//ash
                TeamId = "teams/1946",
                Points = 68,
                GamesPlayed = 28,
                GoalsAgainst = 1,
                GoalsFor = 1
            });

            league.Participants.Add(new Participant
            {
                PlayerId = "players/74",//craig
                TeamId = "teams/1858",
                Points = 37,
                GamesPlayed = 28,
                GoalsAgainst = 1,
                GoalsFor = 1
            });

            league.Participants.Add(new Participant
            {
                PlayerId = "players/75",//daveb
                TeamId = "teams/1405",
                Points = 46,
                GamesPlayed = 28,
                GoalsAgainst = 1,
                GoalsFor = 1
            });

            league.Participants.Add(new Participant
            {
                PlayerId = "players/76",//neil
                TeamId = "1377",
                Points = 29,
                GamesPlayed = 28,
                GoalsAgainst = 1,
                GoalsFor = 1
            });


            league.Participants.Add(new Participant
            {
                PlayerId = "players/77",//luke
                TeamId = "teams/1468",
                Points = 36,
                GamesPlayed = 28,
                GoalsAgainst = 1,
                GoalsFor = 1
            });

            league.Participants.Add(new Participant
            {
                PlayerId = "players/78",//adrian
                TeamId = "teams/1553",
                Points = 23,
                GamesPlayed = 28,
                GoalsAgainst = 1,
                GoalsFor = 1
            });

            league.Participants.Add(new Participant
            {
                PlayerId = "players/79",//jakub
                TeamId = "teams/1413",
                Points = 40,
                GamesPlayed = 28,
                GoalsAgainst = 1,
                GoalsFor = 1
            });

            using (var session = _documentStore.OpenSession())
            {
                session.Store(league);
                session.SaveChanges();
            }

        }
    }
}
