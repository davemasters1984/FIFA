using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FIFA.WebApi.Models.Slack;
using FIFA.QueryServices.Interface;
using System.Text;

namespace FIFA.WebApi.Infrastructure.Slack.Processors
{
    public class GetFixturesSlackRequestProcessor : SlackRequestProcessor
    {
        private ILeagueQueryService _leagueQueryService;
        private string _face;

        public GetFixturesSlackRequestProcessor(ILeagueQueryService leagueQueryService)
        {
            _leagueQueryService = leagueQueryService;
        }

        public override string CommandText
        {
            get
            {
                return "fixtures";
            }
        }

        protected override void ExecuteRequest(SlackRequest request)
        {
            SetDataFromCommandText(request.text);

            var leagueId = _leagueQueryService.GetCurrentLeagueIdForPlayer(_face);

            var fixtures = _leagueQueryService.GetFixturesForPlayerByFace(leagueId, _face);

            var responseString = new StringBuilder();

            foreach (var fixture in fixtures)
                responseString.AppendFormat("\n{0}{1} {2} *vs* {3} {4}{5}",
                    fixture.HomePlayerFace,
                    fixture.HomeTeamBadge,
                    fixture.HomeTeamName,
                    fixture.AwayTeamName,
                    fixture.AwayTeamBadge,
                    fixture.AwayPlayerFace);

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
            return ValidationResult.ValidResult(string.Format("`Retreiving fixtures for:` {0}",
                _face,
                request.user_name));
        }
    }
}