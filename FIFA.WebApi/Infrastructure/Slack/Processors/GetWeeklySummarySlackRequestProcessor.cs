using FIFA.QueryServices.Interface;
using FIFA.WebApi.Models.Slack;
using System;
using System.Text;

namespace FIFA.WebApi.Infrastructure.Slack.Processors
{
    public class GetWeeklySummarySlackRequestProcessor : SlackRequestProcessor
    {
        private IStatisticQueryService _queryService;
        private ILeagueQueryService _leagueQueryService;
        private string _leagueName;

        public override string CommandText
        {
            get
            {
                return "week";
            }
        }

        public GetWeeklySummarySlackRequestProcessor(IStatisticQueryService queryService, ILeagueQueryService leagueQueryService)
        {
            _queryService = queryService;
            _leagueQueryService = leagueQueryService;
        }

        protected override void ExecuteRequest(SlackRequest request)
        {
            var leagueId = _leagueQueryService.GetCurrentLeagueIdFromLeagueName(_leagueName);
            var summary = _queryService.GetWeeklySummary(leagueId);

            var builder = new StringBuilder();

            builder.AppendFormat("Statistics for period {0} to {1}", 
                summary.PeriodStart.ToString("dd MMM"), 
                summary.PeriodEnd.ToString("dd MMM"));

            builder.AppendFormat("\nMost Games Played: {0} `[{1} games played]`",
                summary.PlayerWithMostGamesPlayed.Face,
                summary.PlayerWithMostGamesPlayed.GamesPlayed);

            builder.AppendFormat("\nLeast Games Played: {0} `[{1} games played]`",
                summary.PlayerWithLeastGamesPlayed.Face,
                summary.PlayerWithLeastGamesPlayed.GamesPlayed);

            builder.AppendFormat("\nMost Goals Scored: {0} `[{1} goals] [{2} games played]`", 
                summary.PlayerWithMostGoals.Face, 
                summary.PlayerWithMostGoals.KeyStat, 
                summary.PlayerWithMostGoals.GamesPlayed);

            builder.AppendFormat("\nMost Goals Conceded: {0} `[{1} conceded] [{2} games played]`", 
                summary.PlayerWithMostGoalsConceded.Face,
                summary.PlayerWithMostGoalsConceded.KeyStat,
                summary.PlayerWithMostGoalsConceded.GamesPlayed);

            builder.AppendFormat("\nMost Points: {0} `[{1} points] [{2} games played]`", 
                summary.PlayerWithMostPoints.Face,
                summary.PlayerWithMostPoints.KeyStat,
                summary.PlayerWithMostPoints.GamesPlayed);

            builder.AppendFormat("\nBest Attack: {0} `[{1:F1} goals per game] [{2} games played]`",
                summary.PlayerWithBestAttack.Face,
                summary.PlayerWithBestAttack.KeyStat,
                summary.PlayerWithBestAttack.GamesPlayed);

            builder.AppendFormat("\nWorst Attack: {0} `[{1:F1} goals per game] [{2} games played]`",
                summary.PlayerWithWorstAttack.Face,
                summary.PlayerWithWorstAttack.KeyStat,
                summary.PlayerWithWorstAttack.GamesPlayed);

            builder.AppendFormat("\nBest Defence: {0} `[{1:F1} goals conceded per game] [{2} games played]`",
                summary.PlayerWithBestDefence.Face,
                summary.PlayerWithBestDefence.KeyStat,
                summary.PlayerWithBestDefence.GamesPlayed);

            builder.AppendFormat("\nWorst Defence: {0} `[{1:F1} goals conceded per game] [{2} games played]`",
                summary.PlayerWithWorstDefence.Face,
                summary.PlayerWithWorstDefence.KeyStat,
                summary.PlayerWithWorstDefence.GamesPlayed);

            var response = builder.ToString();

            SendResponse(request.response_url, response);
        }

        public override ValidationResult ValidateRequest(SlackRequest request)
        {
            string[] commandWords = request.text.Split();

            if (commandWords.Length < 2)
                throw new Exception("`You must specify the league name`");

            _leagueName = commandWords[1];

            return ValidationResult.ValidResult("`Fetching weekly summary`");
        }
    }
}