using FIFA.QueryServices.Interface;
using FIFA.WebApi.Models.Slack;
using System;
using System.Text;

namespace FIFA.WebApi.Infrastructure.Slack.Processors
{
    public class GetResultsSlackRequestProcessor : SlackRequestProcessor
    {
        private ILeagueQueryService _leagueQueryService;
        private string _face;

        public override string CommandText
        {
            get
            {
                return "results";
            }
        }

        public GetResultsSlackRequestProcessor(ILeagueQueryService leagueQueryService)
        {
            _leagueQueryService = leagueQueryService;
        }

        public override void Execute(SlackRequest request)
        {
            SetDataFromCommandText(request.text);

            var leagueId = _leagueQueryService.GetCurrentLeagueId();

            var results = _leagueQueryService.GetResultsForPlayerByFace(leagueId, _face);

            var responseString = new StringBuilder();

            foreach (var result in results)
                responseString.AppendFormat("\n{0} {1} *vs* {2} {3}",
                    result.HomePlayerFace,
                    result.HomePlayerGoals,
                    result.AwayPlayerGoals,
                    result.AwayPlayerFace);

            SendResponse(request.response_url, responseString.ToString());
        }

        private void SetDataFromCommandText(string commandText)
        {
            //fifaleague fixtures :dave: 

            string[] commandWords = commandText.Split();

            if (commandText.Length < 6)
                throw new Exception("Invalid Command");

            _face = commandWords[1];
        }

        public override ValidationResult ValidateRequest(SlackRequest request)
        {
            SetDataFromCommandText(request.text);

            return ValidationResult.ValidResult(string.Format("`Retreiving results for` {0}", _face));
        }
    }
}