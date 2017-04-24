using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FIFA.WebApi.Models.Slack;
using FIFA.QueryServices.Interface;
using System.Text;
using FIFA.QueryServices.Interface.Models;

namespace FIFA.WebApi.Infrastructure.Slack.Processors
{
    public class GetFormTableSlackRequestProcessor : SlackRequestProcessor
    {
        private ILeagueQueryService _leagueQueryService;
        private string _leagueName;
        private int _numberOfGames;

        public GetFormTableSlackRequestProcessor(ILeagueQueryService leagueQueryService)
        {
            _leagueQueryService = leagueQueryService;
        }

        public override string CommandText
        {
            get
            {
                return "form";
            }
        }

        protected override void ExecuteRequest(SlackRequest request)
        {
            var currentLeagueId = _leagueQueryService.GetCurrentLeagueIdFromLeagueName(_leagueName);

            var form = (_numberOfGames == 0)
                ? _leagueQueryService.GetFormTable(currentLeagueId)
                : _leagueQueryService.GetFormTable(currentLeagueId, _numberOfGames);

            var response = GetFormTableResponseText(form);

            SendResponse(request.response_url, response.ToString());
        }

        private string GetFormTableResponseText(IEnumerable<FormTableRow> form)
        {
            var response = new StringBuilder();
            var games = (_numberOfGames > 0) ? _numberOfGames : 6;

            response.AppendFormat("\n`Form table for last {0} games:`", games);

            foreach (var formRow in form)
            {
                response.AppendFormat("\n{0}{1}", formRow.PlayerFace, formRow.TeamBadge);

                foreach(var res in formRow.Results.Reverse())
                    response.Append(GetIconForResult(formRow.PlayerId, res));

                response.AppendFormat(" {0}", GetPointsDescription(formRow));
            }

            var responseString = response.ToString();

            return responseString;
        }

        private string GetPointsDescription(FormTableRow row)
        {
            if (row.Results == null)
                return string.Empty;
            if (!row.Results.Any())
                return string.Empty;

            int possiblePoints = 3 * row.Results.Count();

            return string.Format("`[{0} points from a possible {1}]`", row.TotalPoints, possiblePoints);
        }

        private string GetIconForResult(string playerId, Res res)
        {
            if (res == null)
                return string.Empty;

            var points = (res.HomePlayerId == playerId) ? res.HomePoints : res.AwayPoints;

            if (points == 3)
                return ":win:";
            if (points == 1)
                return ":draw:";

            return ":lose:";
        }

        public override ValidationResult ValidateRequest(SlackRequest request)
        {
            SetDataFromCommandText(request.text);

            if (_numberOfGames > 0)
                return ValidationResult.ValidResult(string.Format("`Fetching form table for last {0} games for {1}`", _numberOfGames, _leagueName));

            return ValidationResult.ValidResult(string.Format("`Fetching form table for last 6 games for {0}`", _leagueName));
        }

        private void SetDataFromCommandText(string commandText)
        {
            string[] commandWords = commandText.Split();

            if (commandWords.Length < 2)
                throw new Exception("You must specify the league name");

            _leagueName = commandWords[1];

            if (commandWords.Length < 3)
                return;

            if (!int.TryParse(commandWords[2], out _numberOfGames))
                throw new Exception(string.Format("Could not parse {0} as a number", commandWords[2]));
        }
    }
}