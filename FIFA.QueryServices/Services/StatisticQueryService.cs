using FIFA.Model;
using FIFA.QueryServices.Indexes;
using FIFA.QueryServices.Interface;
using FIFA.QueryServices.Interface.Models;
using Raven.Client;
using System;
using System.Linq;

namespace FIFA.QueryServices.Services
{
    public class StatisticQueryService : IStatisticQueryService
    {
        private IDocumentStore _documentStore;

        public StatisticQueryService(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public void GenerateStatisticsForPeriod(GenerateStatisticsForPeriodArgs args)
        {
            using (var session = _documentStore.OpenSession())
            {
                var oneWeekAgo = DateTime.Now.Date.AddDays(-7);

                var allResultsForPastWeek = session.Query<ResultSummary, ResultsIndex>()
                    .Where(r => r.LeagueId == args.LeagueId)
                    .Where(r => r.Date > oneWeekAgo)
                    .ToList();
                
                var homePlayerStats = allResultsForPastWeek
                    .GroupBy(r => r.HomePlayerId, r => new
                    {
                        GoalsScored = r.HomePlayerGoals,
                        GoalsConceded = r.AwayPlayerGoals,
                        Points = (r.HomePlayerGoals > r.AwayPlayerGoals) ? 3 : (r.HomePlayerGoals == r.AwayPlayerGoals) ? 1 : 0,
                        GamesPlayed = 1
                    },
                    (key, group) => new PlayerStatisticsSummary
                    {
                        PlayerId = key,
                        GoalsScored = group.Sum(g => g.GoalsScored),
                        GoalsConceded = group.Sum(g => g.GoalsConceded),
                        Points = group.Sum(g => g.Points),
                        GamesPlayed = group.Sum(g => g.GamesPlayed)
                    })
                    .ToList();

                var awayPlayerStats = allResultsForPastWeek
                    .GroupBy(r => r.AwayPlayerId, r => new
                    {
                        GoalsScored = r.AwayPlayerGoals,
                        GoalsConceded = r.HomePlayerGoals,
                        Points = (r.AwayPlayerGoals > r.HomePlayerGoals) ? 3 : (r.HomePlayerGoals == r.AwayPlayerGoals) ? 1 : 0,
                        GamesPlayed = 1
                    },
                    (key, group) => new PlayerStatisticsSummary
                    {
                        PlayerId = key,
                        GoalsScored = group.Sum(g => g.GoalsScored),
                        GoalsConceded = group.Sum(g => g.GoalsConceded),
                        Points = group.Sum(g => g.Points),
                        GamesPlayed = group.Sum(g => g.GamesPlayed),
                    })
                    .ToList();

                var playerStats = homePlayerStats
                    .Union(awayPlayerStats)
                    .GroupBy(r => r.PlayerId, (key, group) => new PlayerStatisticsSummary
                    {
                        PlayerId = key,
                        GoalsScored = group.Sum(g => g.GoalsScored),
                        GoalsConceded = group.Sum(g => g.GoalsConceded),
                        GamesPlayed = group.Sum(g => g.GamesPlayed),
                        Points = group.Sum(g => g.Points),
                    })
                    .ToList();

                var leagueStats = new LeagueStatisticsForPeriod
                {
                    PeriodStart = args.PeriodStart,
                    PeriodEnd = args.PeriodEnd,
                    LeagueId = args.LeagueId,
                    DateCreated = DateTime.Now,
                    PlayerStatistics = playerStats
                };

                session.Store(leagueStats);
                session.SaveChanges();
            }
        }

        public StatisticSummary GetWeeklySummary(string leagueId)
        {
            using (var session = _documentStore.OpenSession())
            {
                var playerNames = session.Query<Player>()
                    .Select(p => new { p.Id, p.Face, p.Name })
                    .ToList();

                var latestWeeklyStatistic = session.Query<LeagueStatisticsForPeriod>()
                    .Where(s => s.Days == 7)
                    .Where(s => s.LeagueId == leagueId)
                    .OrderByDescending(s => s.PeriodEnd)
                    .FirstOrDefault();

                var weeklySummary = new StatisticSummary
                {
                    LeagueId = latestWeeklyStatistic.LeagueId,
                    PeriodStart = latestWeeklyStatistic.PeriodStart,
                    PeriodEnd = latestWeeklyStatistic.PeriodEnd
                };

                weeklySummary.PlayerWithMostGoals = latestWeeklyStatistic.PlayerStatistics
                    .OrderByDescending(r => r.GoalsScored)
                    .Select(r => new PlayerStatistic { PlayerId = r.PlayerId, KeyStat = r.GoalsScored, GamesPlayed = r.GamesPlayed, Face = playerNames.Where(p => p.Id == r.PlayerId).Select(p => p.Face).FirstOrDefault() })
                    .FirstOrDefault();

                weeklySummary.PlayerWithMostGoalsConceded = latestWeeklyStatistic.PlayerStatistics
                    .OrderByDescending(r => r.GoalsConceded)
                    .Select(r => new PlayerStatistic { PlayerId = r.PlayerId, KeyStat = r.GoalsConceded, GamesPlayed = r.GamesPlayed, Face = playerNames.Where(p => p.Id == r.PlayerId).Select(p => p.Face).FirstOrDefault() })
                    .FirstOrDefault();

                weeklySummary.PlayerWithMostGamesPlayed = latestWeeklyStatistic.PlayerStatistics
                    .OrderByDescending(r => r.GamesPlayed)
                    .Select(r => new PlayerStatistic { PlayerId = r.PlayerId, GamesPlayed = r.GamesPlayed, Face = playerNames.Where(p => p.Id == r.PlayerId).Select(p => p.Face).FirstOrDefault() })
                    .FirstOrDefault();

                weeklySummary.PlayerWithMostPoints = latestWeeklyStatistic.PlayerStatistics
                    .OrderByDescending(r => r.Points)
                    .Select(r => new PlayerStatistic { PlayerId = r.PlayerId, KeyStat = r.Points, GamesPlayed = r.GamesPlayed, Face = playerNames.Where(p => p.Id == r.PlayerId).Select(p => p.Face).FirstOrDefault() })
                    .FirstOrDefault();

                weeklySummary.PlayerWithLeastPoints = latestWeeklyStatistic.PlayerStatistics
                    .OrderBy(r => r.Points)
                    .Select(r => new PlayerStatistic { PlayerId = r.PlayerId, KeyStat = r.Points, GamesPlayed = r.GamesPlayed, Face = playerNames.Where(p => p.Id == r.PlayerId).Select(p => p.Face).FirstOrDefault() })
                    .FirstOrDefault();

                weeklySummary.PlayerWithLeastGamesPlayed = latestWeeklyStatistic.PlayerStatistics
                    .OrderBy(r => r.GamesPlayed)
                    .Select(r => new PlayerStatistic { PlayerId = r.PlayerId, KeyStat = r.GamesPlayed, GamesPlayed = r.GamesPlayed, Face = playerNames.Where(p => p.Id == r.PlayerId).Select(p => p.Face).FirstOrDefault() })
                    .FirstOrDefault();

                weeklySummary.PlayerWithBestAttack = latestWeeklyStatistic.PlayerStatistics
                    .OrderByDescending(r => r.GoalsScored / r.GamesPlayed)
                    .Select(r => new PlayerStatistic { PlayerId = r.PlayerId, KeyStat = ((double)r.GoalsScored / (double)r.GamesPlayed), GamesPlayed = r.GamesPlayed, Face = playerNames.Where(p => p.Id == r.PlayerId).Select(p => p.Face).FirstOrDefault() })
                    .FirstOrDefault();

                weeklySummary.PlayerWithWorstAttack = latestWeeklyStatistic.PlayerStatistics
                    .OrderBy(r => r.GoalsScored / r.GamesPlayed)
                    .Select(r => new PlayerStatistic { PlayerId = r.PlayerId, KeyStat = ((double)r.GoalsScored / (double)r.GamesPlayed), GamesPlayed = r.GamesPlayed, Face = playerNames.Where(p => p.Id == r.PlayerId).Select(p => p.Face).FirstOrDefault() })
                    .FirstOrDefault();

                weeklySummary.PlayerWithBestDefence = latestWeeklyStatistic.PlayerStatistics
                    .OrderBy(r => r.GoalsConceded / r.GamesPlayed)
                    .Select(r => new PlayerStatistic { PlayerId = r.PlayerId, KeyStat = ((double)r.GoalsConceded / (double)r.GamesPlayed), GamesPlayed = r.GamesPlayed, Face = playerNames.Where(p => p.Id == r.PlayerId).Select(p => p.Face).FirstOrDefault() })
                    .FirstOrDefault();

                weeklySummary.PlayerWithWorstDefence = latestWeeklyStatistic.PlayerStatistics
                    .OrderByDescending(r => r.GoalsConceded / r.GamesPlayed)
                    .Select(r => new PlayerStatistic { PlayerId = r.PlayerId, KeyStat = ((double)r.GoalsConceded / (double)r.GamesPlayed), GamesPlayed = r.GamesPlayed, Face = playerNames.Where(p => p.Id == r.PlayerId).Select(p => p.Face).FirstOrDefault() })
                    .FirstOrDefault();

                return weeklySummary;
            }
        }
    }
}
