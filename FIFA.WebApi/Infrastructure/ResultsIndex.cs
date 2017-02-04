using FIFA.Model;
using FIFA.WebApi.Models;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FIFA.WebApi.Infrastructure
{
    public class ResultsIndex : AbstractIndexCreationTask<Result, ResultSummary>
    {
        public ResultsIndex()
        {
            Map =
                results =>
                from result in results
                let homePlayer = LoadDocument<Player>(result.HomePlayerId)
                let awayPlayer = LoadDocument<Player>(result.AwayPlayerId)
                select new ResultSummary
                {
                    LeagueId = result.LeagueId,
                    HomePlayerId = homePlayer.Id,
                    HomePlayerFace = homePlayer.Face,
                    HomePlayerName = homePlayer.Name,
                    HomePlayerGoals = result.HomePlayerGoals,
                    AwayPlayerId = awayPlayer.Id,
                    AwayPlayerFace = awayPlayer.Face,
                    AwayPlayerName = awayPlayer.Name,
                    AwayPlayerGoals = result.AwayPlayerGoals,
                };

            Reduce =
                results => from r in results
                           group r by new
                           {
                               LeagueId = r.LeagueId,
                               HomePlayerId = r.HomePlayerId,
                               HomePlayerFace = r.HomePlayerFace,
                               HomePlayerName = r.HomePlayerName,
                               HomePlayerGoals = r.HomePlayerGoals,
                               AwayPlayerId = r.AwayPlayerId,
                               AwayPlayerFace = r.AwayPlayerFace,
                               AwayPlayerName = r.AwayPlayerName,
                               AwayPlayerGoals = r.AwayPlayerGoals,
                           } 
                           into g
                           select new ResultSummary
                           {
                               LeagueId = g.Key.LeagueId,
                               HomePlayerId = g.Key.HomePlayerId,
                               HomePlayerFace = g.Key.HomePlayerFace,
                               HomePlayerName = g.Key.HomePlayerName,
                               HomePlayerGoals = g.Key.HomePlayerGoals,
                               AwayPlayerId = g.Key.AwayPlayerId,
                               AwayPlayerFace = g.Key.AwayPlayerFace,
                               AwayPlayerName = g.Key.AwayPlayerName,
                               AwayPlayerGoals = g.Key.AwayPlayerGoals,
                           };
        }
    }
}