using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FIFA.WebApi.Models.Slack;
using Microsoft.AspNet.WebHooks;
using FIFA.QueryServices.Interface;

namespace FIFA.WebApi.Infrastructure.Slack.Processors
{
    public class GetPlayerComparisonHistorySlackRequestProcessor : SlackRequestProcessor
    {
        private string _playerOneFace;
        private string _playerTwoFace;
        private string _orginalRequestUrl;
        private ILeagueQueryService _queryService;
        public override string CommandText
        {
            get
            {
                return "compare";
            }
        }

        public GetPlayerComparisonHistorySlackRequestProcessor(ILeagueQueryService queryService)
        {
            _queryService = queryService;
        }

        public override void Execute(SlackRequest request)
        {
            var response = new SlackSlashResponse(string.Empty);

            var data = _queryService.GetCurrentLeagueAndPlayerIds(_playerOneFace, _playerTwoFace);

            var playerOneId = GetIdWithoutPrefix("players/", data.PlayerOneId);
            var playerTwoId = GetIdWithoutPrefix("players/", data.PlayerTwoId);
            var leagueId = GetIdWithoutPrefix("leagues/", data.LeagueId);

            var graphUrl = string.Format("{0}/api/leagues/{1}/players/{2}/compare/{3}",
                _orginalRequestUrl,
                leagueId,
                playerOneId,
                playerTwoId);

            response.Attachments.Add(new SlackAttachment("Player Position History", "")
            {
                ImageLink = new Uri(graphUrl)
            });

            SendResponse(request.response_url, response);
        }

        private string GetIdWithoutPrefix(string prefix, string id)
        {
            return id.Replace(prefix, string.Empty);
        }

        public override ValidationResult ValidateRequest(SlackRequest request)
        {
            _orginalRequestUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

            GetCommandData(request.text);

            return ValidationResult.ValidResult("`Retreiving player position comparison chart`");
        }

        private void GetCommandData(string commandText)
        {
            string[] commandWords = commandText.Split();

            if (commandText.Length < 3)
                throw new Exception("Invalid Command");

            _playerOneFace = commandWords[1];
            _playerTwoFace = commandWords[2];
            
        }
    }
}