using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FIFA.WebApi.Models.Slack;
using Microsoft.AspNet.WebHooks;
using FIFA.QueryServices.Interface;

namespace FIFA.WebApi.Infrastructure.Slack.Processors
{
    public class GetPlayerPositionChartSlackRequestProcessor : SlackRequestProcessor
    {
        private List<string> _playerFaces;
        private List<string> _playerIds;
        private string _orginalRequestUrl;
        private ILeagueQueryService _queryService;
        private IPlayerQueryService _playerQueryService;

        public override string CommandText
        {
            get { return "chart"; }
        }

        public GetPlayerPositionChartSlackRequestProcessor(ILeagueQueryService queryService,
            IPlayerQueryService playerQueryService)
        {
            _queryService = queryService;
            _playerQueryService = playerQueryService;
        }

        protected override void ExecuteRequest(SlackRequest request)
        {
            var response = new SlackSlashResponse(string.Empty);

            var fullLeagueId = _queryService.GetCurrentLeagueId();

            var leagueId = GetIdWithoutPrefix("leagues/", fullLeagueId);

            var graphUrl = string.Format("{0}/api/leagues/{1}/position-history/{2}",
                _orginalRequestUrl,
                leagueId,
                string.Join(",", _playerIds));

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

            try
            {
                SetCommandData(request.text);
                ResolvePlayerIds();

                return ValidationResult.ValidResult("`Retreiving player position comparison chart`");
            }
            catch (Exception ex)
            {
                return ValidationResult.InvalidResult(string.Format("`Unable to add result: {0}`", ex.Message));
            }
        }

        private void ResolvePlayerIds()
        {
            _playerIds = new List<string>();

            foreach(var face in _playerFaces)
            {
                var id = _playerQueryService.ResolvePlayerId(face);
                
                _playerIds.Add(GetIdWithoutPrefix("players/", id));
            }
        }

        private void SetCommandData(string commandText)
        {
            string[] commandWords = commandText.Split();

            if (commandWords.Length < 2)
                throw new Exception("Invalid Command");

            _playerFaces = new List<string>();

            foreach (string face in commandWords.Skip(1))
                _playerFaces.Add(face);           
        }
    }
}