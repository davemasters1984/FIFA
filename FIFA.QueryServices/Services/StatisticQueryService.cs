using FIFA.Model;
using FIFA.QueryServices.Indexes;
using FIFA.QueryServices.Interface;
using FIFA.QueryServices.Interface.Models;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Services
{
    public class StatisticQueryService : IStatisticQueryService
    {
        private IDocumentStore _documentStore;

        public StatisticQueryService(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public void GenerateStatistics()
        {
            using (var session = _documentStore.OpenSession())
            {
                var oneWeekAgo = DateTime.Now.Date.AddDays(-7);

                var allResultsForPastWeek = session.Query<ResultSummary, ResultsIndex>()
                    .Where(r => r.LeagueId == "leagues/417")
                    .Where(r => r.Date > oneWeekAgo)
                    .ToList();
                
                var homePlayerWeeklyResultsSummary = allResultsForPastWeek
                    .GroupBy(r => r.HomePlayerId, r => new
                    {
                        GoalsScored = r.HomePlayerGoals,
                        GoalsConceded = r.AwayPlayerGoals,
                        Points = (r.HomePlayerGoals > r.AwayPlayerGoals) ? 3 : (r.HomePlayerGoals == r.AwayPlayerGoals) ? 1 : 0,
                        GamesPlayed = 1
                    },
                    (key, group) => new PlayerWeeklyResultsSummary
                    {
                        PlayerId = key,
                        GoalsScored = group.Sum(g => g.GoalsScored),
                        GoalsConceded = group.Sum(g => g.GoalsConceded),
                        Points = group.Sum(g => g.Points),
                        GamesPlayed = group.Sum(g => g.GamesPlayed)
                    })
                    .ToList();

                var awayPlayerWeeklyResultsSummary = allResultsForPastWeek
                    .GroupBy(r => r.AwayPlayerId, r => new
                    {
                        GoalsScored = r.AwayPlayerGoals,
                        GoalsConceded = r.HomePlayerGoals,
                        Points = (r.AwayPlayerGoals > r.HomePlayerGoals) ? 3 : (r.HomePlayerGoals == r.AwayPlayerGoals) ? 1 : 0,
                        GamesPlayed = 1
                    },
                    (key, group) => new PlayerWeeklyResultsSummary
                    {
                        PlayerId = key,
                        GoalsScored = group.Sum(g => g.GoalsScored),
                        GoalsConceded = group.Sum(g => g.GoalsConceded),
                        Points = group.Sum(g => g.Points),
                        GamesPlayed = group.Sum(g => g.GamesPlayed)
                    })
                    .ToList();

                var playerWeeklySummary = homePlayerWeeklyResultsSummary
                    .Union(awayPlayerWeeklyResultsSummary)
                    .GroupBy(r => r.PlayerId, (key, group) => new PlayerWeeklyResultsSummary
                    {
                        PlayerId = key,
                        GoalsScored = group.Sum(g => g.GoalsScored),
                        GoalsConceded = group.Sum(g => g.GoalsConceded),
                        GamesPlayed = group.Sum(g => g.GamesPlayed),
                        Points = group.Sum(g => g.Points)

                    }).ToList();

                foreach(var summary in playerWeeklySummary)
                    session.Store(summary);

                session.SaveChanges();
            }
        }

        public WeeklySummary GetWeeklySummary()
        {
            using (var session = _documentStore.OpenSession())
            {
                var playerNames = session.Query<Player>()
                    .Select(p => new { p.Id, p.Face, p.Name })
                    .ToList();

                var resultsSummary = session.Query<PlayerWeeklyResultsSummary>()
                    .ToList();

                var weeklySummary = new WeeklySummary();

                weeklySummary.PlayerWithMostGoals = resultsSummary
                    .OrderByDescending(r => r.GoalsScored)
                    .Select(r => new WeeklyStatistic { PlayerId = r.PlayerId, KeyStat = r.GoalsScored, GamesPlayed = r.GamesPlayed, Face = playerNames.Where(p => p.Id == r.PlayerId).Select(p => p.Face).FirstOrDefault() })
                    .FirstOrDefault();

                weeklySummary.PlayerWithLeastPoints = resultsSummary
                    .OrderByDescending(r => r.GoalsConceded)
                    .Select(r => new WeeklyStatistic { PlayerId = r.PlayerId, KeyStat = r.GoalsConceded, GamesPlayed = r.GamesPlayed, Face = playerNames.Where(p => p.Id == r.PlayerId).Select(p => p.Face).FirstOrDefault() })
                    .FirstOrDefault();

                weeklySummary.PlayerWithMostGamesPlayed = resultsSummary
                    .OrderByDescending(r => r.GoalsConceded)
                    .Select(r => new WeeklyStatistic { PlayerId = r.PlayerId, GamesPlayed = r.GamesPlayed, Face = playerNames.Where(p => p.Id == r.PlayerId).Select(p => p.Face).FirstOrDefault() })
                    .FirstOrDefault();

                weeklySummary.PlayerWithMostPoints = resultsSummary
                    .OrderByDescending(r => r.Points)
                    .Select(r => new WeeklyStatistic { PlayerId = r.PlayerId, KeyStat = r.Points, GamesPlayed = r.GamesPlayed, Face = playerNames.Where(p => p.Id == r.PlayerId).Select(p => p.Face).FirstOrDefault() })
                    .FirstOrDefault();

                weeklySummary.PlayerWithLeastPoints = resultsSummary
                    .OrderBy(r => r.Points)
                    .Select(r => new WeeklyStatistic { PlayerId = r.PlayerId, KeyStat = r.Points, GamesPlayed = r.GamesPlayed, Face = playerNames.Where(p => p.Id == r.PlayerId).Select(p => p.Face).FirstOrDefault() })
                    .FirstOrDefault();

                return weeklySummary;
            }
        }
    }
}
