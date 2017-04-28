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
        private string _faceOrLeagueName;

        private bool IsByFace
        {
            get
            {
                if (_faceOrLeagueName.Contains(":"))
                    return true;

                return false;
            }
        }

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

        public override string ExampleRequest
        {
            get
            {
                return $"`{SlackSlashCommand} {CommandText} :dave:`";
            }
        }

        public override string Description
        {
            get
            {
                return "Returns the list of remaining fixtures for the player";
            }
        }

        protected override void ExecuteRequest(SlackRequest request)
        {
            var leagueId = GetLeagueId();

            var fixtures = (IsByFace)
                ? _leagueQueryService.GetFixturesForPlayerByFace(leagueId, _faceOrLeagueName)
                : _leagueQueryService.GetFixturesForLeagueId(leagueId);

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

        private string GetLeagueId()
        {
            if (IsByFace)
                return _leagueQueryService.GetCurrentLeagueIdForPlayer(_faceOrLeagueName);
            else
                return _leagueQueryService.GetCurrentLeagueIdFromLeagueName(_faceOrLeagueName);
        }

        private void SetDataFromCommandText(string commandText)
        {
            //fifaleague fixtures :dave: 

            string[] commandWords = commandText.Split();

            if (commandText.Length < 6)
                throw new Exception("Invalid Command");

            _faceOrLeagueName = commandWords[1];
        }

        public override ValidationResult ValidateRequest(SlackRequest request)
        {
            SetDataFromCommandText(request.text);

            return ValidationResult.ValidResult(string.Format("`Retreiving fixtures for:` {0}",
                _faceOrLeagueName,
                request.user_name));
        }
    }
}