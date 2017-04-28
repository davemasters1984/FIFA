using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FIFA.WebApi.Models.Slack;
using FIFA.QueryServices.Interface;
using System.Text;

namespace FIFA.WebApi.Infrastructure.Slack.Processors
{
    public class GetHeadToHeadForLeagueSlackRequestProcessor : SlackRequestProcessor
    {
        private string _faceOne;
        private string _faceTwo;
        private ILeagueQueryService _leagueQueryService;

        public override string CommandText
        {
            get
            {
                return "h2h";
            }
        }

        public override string ExampleRequest
        {
            get
            {
                return $"`{SlackSlashCommand} {CommandText} :dave: :dom:`";
            }
        }

        public override string Description
        {
            get
            {
                return "Returns previous results between two players";
            }
        }

        public GetHeadToHeadForLeagueSlackRequestProcessor(ILeagueQueryService leagueQueryService)
        {
            _leagueQueryService = leagueQueryService;
        }

        public override ValidationResult ValidateRequest(SlackRequest request)
        {
            SetDataFromCommandText(request.text);

            return ValidationResult.ValidResult(string.Format("`Retreiving results for:` {0} v {1}",
                _faceOne,
                _faceTwo));
        }

        protected override void ExecuteRequest(SlackRequest request)
        {
            var leagueId = _leagueQueryService.GetCurrentLeagueId();

            var results = _leagueQueryService.GetHeadToHeadResults(leagueId, _faceOne, _faceTwo);

            var responseString = new StringBuilder();

            responseString.AppendFormat("\n`Head-to-head results for` {0} vs {1}", _faceOne, _faceTwo);

            foreach (var result in results.OrderBy(r => r.Date))
                responseString.AppendFormat("\n{0} {1} *vs* {2} {3} `[{4}]`",
                    result.HomePlayerFace,
                    result.HomePlayerGoals,
                    result.AwayPlayerGoals,
                    result.AwayPlayerFace,
                    result.Date.ToString("dd MMM"));

            SendResponse(request.response_url, responseString.ToString());
        }

        private void SetDataFromCommandText(string commandText)
        {
            string[] commandWords = commandText.Split();

            if (commandText.Length < 6)
                throw new Exception("Invalid Command");

            _faceOne = commandWords[1];
            _faceTwo = commandWords[2];
        }
    }
}