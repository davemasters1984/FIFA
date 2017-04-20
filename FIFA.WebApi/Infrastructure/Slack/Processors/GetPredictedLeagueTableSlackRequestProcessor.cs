using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FIFA.WebApi.Models.Slack;
using FIFA.QueryServices.Interface;
using System.Text;

namespace FIFA.WebApi.Infrastructure.Slack.Processors
{
    public class GetPredictedLeagueTableSlackRequestProcessor : SlackRequestProcessor
    {
        private IStatisticQueryService _queryService;

        public override string CommandText
        {
            get
            {
                return "predicted";
            }
        }

        public GetPredictedLeagueTableSlackRequestProcessor(IStatisticQueryService queryService)
        {
            _queryService = queryService;
        }

        public override ValidationResult ValidateRequest(SlackRequest request)
        {
            return ValidationResult.ValidResult("`Retreiving predicted table`");
        }

        protected override void ExecuteRequest(SlackRequest request)
        {
            var predictedTable = _queryService.GetPredictedTable("leagues/417");

            var response = new StringBuilder();
            int currentPosition = 0;
            int relegationPosition = predictedTable.Count();
            int relegationPlayOffPosition = predictedTable.Count() - 2;
            var relegationIcon = ":skull:";
            var relegationPlayOffIcon = ":scream:";
            var championIcon = ":crown:";

            response.Append("`Predicted Final Table:`");

            foreach (var row in predictedTable)
            {
                currentPosition++;
                var positionString = GetFormattedNumberString(currentPosition);

                var pointsString = GetFormattedNumberString(row.Points);

                var icon = (currentPosition >= relegationPosition)
                    ? relegationIcon
                    : (currentPosition >= relegationPlayOffPosition)
                        ? relegationPlayOffIcon
                        : string.Empty;

                if (currentPosition == 1)
                    icon = championIcon;

                if (currentPosition == relegationPosition || currentPosition == relegationPlayOffPosition)
                    response.Append("\n-----------------------------------------------------------------------------------");

                response.AppendFormat(string.Format("\n{0}{1} {2} Pts: *{3}* {4}",
                    positionString,
                    row.PlayerFace,
                    row.TeamBadge,
                    pointsString,
                    icon));
            }

            var responseString = response.ToString();

            SendResponse(request.response_url, responseString);
        }

        private string GetGoalDifferenceNumberString(int goalDifference)
        {
            if (goalDifference < -9)
                return string.Format("*{0}*", goalDifference);
            if (goalDifference > 9)
                return string.Format(" *{0}*", goalDifference);
            if (goalDifference < 0)
                return string.Format("*{0}* ", goalDifference);

            return string.Format(" *{0}*  ", goalDifference);
        }
    }
}