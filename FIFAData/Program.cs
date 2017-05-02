using FIFA.CommandServices;
using FIFA.Infrastructure;
using FIFA.Model;
using FIFA.Model.Services;
using FIFA.QueryServices.Indexes;
using FIFA.QueryServices.Interface.Models;
using FIFA.QueryServices.Models;
using FIFA.QueryServices.Services;
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
        static IEnumerable<string> _participantNames;

        static void Main(string[] args)
        {
            CreateDocumentStore();
            //GetTeam();
            //ImportTeams();
            ReSaveLeague();

            Console.WriteLine("Complete.");

            Console.Read();
        }

        private static void ReSaveLeague()
        {
            using (var session = _documentStore.OpenSession())
            {
                var league = session.Load<League>("leagues/385");

                league.Name = "premiership";

                session.Store(league);
                session.SaveChanges();
            }
        }

        private static void GetTeam()
        {
            using (var session = _documentStore.OpenSession())
            {
                var teams = session.GetAll<Team>();

                var fern = teams.Where(t => t.TeamName.ToLower().Contains("rangers"));
                var fernTeam = fern.FirstOrDefault();

                Console.WriteLine(fernTeam.TeamName);
            }
        }

        private static void GetWeeklyStatistic()
        {
            var queryService = new StatisticQueryService(_documentStore);

            var stats = queryService.GetWeeklySummary("leagues/417");

            stats.ToString();
        }

        private static void DeployIndexes()
        {
            new LeagueTableIndex().Execute(_documentStore);
            new ResultsIndex().Execute(_documentStore);
            new FixturesIndex().Execute(_documentStore);
            new LeagueSummaryIndex().Execute(_documentStore);
            new FormTableIndex().Execute(_documentStore);
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

        private static void TestIndexWait()
        {
            var leagueQueryService = new LeagueQueryService(_documentStore, new PlayerQueryService(_documentStore));

            var league = leagueQueryService.GetLeagueTableWaitForIndex("leagues/417");
        }

        private static void TakeSnapshot()
        {
            var leagueQueryService = new LeagueQueryService(_documentStore, new PlayerQueryService(_documentStore));

            var currentLeagueId = leagueQueryService.GetCurrentLeagueId();
            var currentLeague = leagueQueryService.GetLeagueTableWaitForIndex(currentLeagueId);
            var currentDate = DateTime.Now.Date;

            using (var session = _documentStore.OpenSession())
            {
                var snapshot = session.Query<LeagueTableSnapshot>()
                    .Where(s => s.SnapshotDate == currentDate)
                    .Where(s => s.LeagueId == currentLeagueId)
                    .FirstOrDefault();

                if (snapshot == null)
                    snapshot = new LeagueTableSnapshot();

                snapshot.Rows = MapSnapshotRows(currentLeague);
                snapshot.SnapshotDate = currentDate;
                snapshot.LeagueId = currentLeagueId;

                session.Store(snapshot);
                session.SaveChanges();
            }
        }

        private static List<SnapshotRow> MapSnapshotRows(IEnumerable<LeagueTableRow> rows)
        {
            var snapshotRows = new List<SnapshotRow>();

            foreach (var row in rows)
                snapshotRows.Add(MapToSnapshotRow(row));

            return snapshotRows;
        }

        private static SnapshotRow MapToSnapshotRow(LeagueTableRow row)
        {
            return new SnapshotRow
            {
                Position = row.Position,
                TeamId = row.TeamId,
                GamesDrawn = row.GamesDrawn,
                GamesLost = row.GamesLost,
                GamesPlayed = row.GamesPlayed,
                GamesWon = row.GamesWon,
                GoalsAgainst = row.GoalsAgainst,
                GoalsFor = row.GoalsFor,
                PlayerFace = row.PlayerFace,
                PlayerName = row.PlayerName,
                PlayerId = row.PlayerId,
                Points = row.Points,
                TeamBadge = row.TeamBadge,
                TeamName = row.TeamName,
                TeamRating = row.TeamRating
            };
        }

        private static void CreateLeague()
        {
            //var leagueService = new LeagueCommandService(new RavenRepository(), new LeagueService(), new ResultService(new RavenRepository()));

            //leagueService.CreateLeague(new FIFA.CommandServices.Interface.CreateLeagueCommand
            //{
            //    ParticipantFaces = _participantNames
            //});
        }      

        private static void QueryForm()
        {
            using (var session = _documentStore.OpenSession())
            {
                var form = session.Query<FormTableRow, FormTableIndex>()
                    .ToList();

                form.ToString();
            }
        }

        private static void UpdateSnapshotPlayerIds()
        {
            using (var session = _documentStore.OpenSession())
            {
                var snapShots = session.Query<LeagueTableSnapshot>().ToList();
                var players = session.Query<Player>().ToList();

                foreach(var snap in snapShots)
                {
                    foreach(var row in snap.Rows)
                    {
                        row.PlayerId = players
                            .Where(p => p.Face == row.PlayerFace)
                            .Select(p => p.Id)
                            .FirstOrDefault();
                    }

                    session.Store(snap);
                }

                session.SaveChanges();
            }
        }

        private static void OutputLatestLeague()
        {
            using (var session = _documentStore.OpenSession())
            {
                var latestLeague = session.Query<League>()
                    .OrderByDescending(l => l.CreatedDate)
                    .FirstOrDefault();

                var leagueTable
                    = session.Query<LeagueTableRow, LeagueTableIndex>()
                        .Where(l => l.LeagueId == latestLeague.Id)
                        .OrderBy(l => l.TeamRating)
                        .ToList();
                
                foreach(var row in leagueTable)
                {
                    Console.WriteLine(string.Format("{0} - {1} [{2}]",
                        row.PlayerName,
                        row.TeamName,
                        row.TeamRating));
                }
            }
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

        private static void AddPlayer(string face, string name)
        {
            var player = new Player
            {
                Face = face,
                Name = name,
                IsNew = true,
            };

            using (var session = _documentStore.OpenSession())
            {
                session.Store(player);
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
                ConnectionStringName = "azure",
            };

            _documentStore.Initialize();
        }

        public static void GenerateStatistics()
        {
            var queryService = new StatisticQueryService(_documentStore);

            queryService.GenerateStatisticsForPeriod(new FIFA.QueryServices.Interface.GenerateStatisticsForPeriodArgs
            {
                LeagueId = "leagues/417",
                PeriodEnd = DateTime.Now.Date,
                PeriodStart = DateTime.Now.Date.AddDays(-7)
            });
        }

        public static void DuplicateLeague(string leagueId)
        {
            using (var session = _documentStore.OpenSession())
            {
                var league = session.Load<League>(leagueId);

                session.Advanced.Evict(league);
                var newId = session.Advanced.GetDocumentId(league);

                session.Store(league, leagueId + "0");
            }
        }

        private static void GenerateFixturesForLeague(string leagueId)
        {
            using (var session = _documentStore.OpenSession())
            {
                var league = session.Load<League>(leagueId);

                if (HasFixtures(league))
                    return;

                FixtureGenerator gen = new FixtureGenerator(league.Participants);

                league.Fixtures = gen.GenerateFixtures();

                session.Store(league);
                session.SaveChanges();
            }
        }

        private static bool HasFixtures(League league)
        {
            if (league.Fixtures == null)
                return false;
            if (!league.Fixtures.Any())
                return false;

            return true;
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
