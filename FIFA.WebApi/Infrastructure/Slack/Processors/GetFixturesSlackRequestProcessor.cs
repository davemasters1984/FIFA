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

        public override void Execute(SlackRequest request)
        {
            SetDataFromCommandText(request.text);

            var leagueId = _leagueQueryService.GetCurrentLeagueId();

            var fixtures = _leagueQueryService.GetFixturesForPlayerByFace(leagueId, _face);

            var responseString = new StringBuilder();

            foreach (var fixture in fixtures)
                responseString.AppendFormat("{0} vs {1}",
                    fixture.HomePlayerFace,
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
    }
}