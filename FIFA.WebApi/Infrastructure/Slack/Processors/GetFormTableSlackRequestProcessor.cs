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

        public override void Execute(SlackRequest request)
        {
            var currentLeagueId = _leagueQueryService.GetCurrentLeagueId();
            var form = _leagueQueryService.GetFormTable(currentLeagueId);

            var response = new StringBuilder();

            foreach(var formRow in form)
            {
                response.AppendFormat("\n{0}{1} {7}{6}{5}{4}{3}{2} {8}",
                    formRow.PlayerFace,
                    formRow.TeamBadge,
                    GetIconForResult(formRow.PlayerId, formRow.Results.Take(1).FirstOrDefault()),
                    GetIconForResult(formRow.PlayerId, formRow.Results.Skip(1).Take(1).FirstOrDefault()),
                    GetIconForResult(formRow.PlayerId, formRow.Results.Skip(2).Take(1).FirstOrDefault()),
                    GetIconForResult(formRow.PlayerId, formRow.Results.Skip(3).Take(1).FirstOrDefault()),
                    GetIconForResult(formRow.PlayerId, formRow.Results.Skip(4).Take(1).FirstOrDefault()),
                    GetIconForResult(formRow.PlayerId, formRow.Results.Skip(5).Take(1).FirstOrDefault()),
                    GetPointsDescription(formRow));
            }

            SendResponse(request.response_url, response.ToString());
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
            return ValidationResult.ValidResult("`Fetching form table`");
        }
    }
}