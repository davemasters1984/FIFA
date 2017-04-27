using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FIFA.WebApi.Models.Slack;
using FIFA.CommandServices.Interface;
using FIFA.QueryServices.Interface;

namespace FIFA.WebApi.Infrastructure.Slack.Processors
{
    public class UpdatePlayerSlackRequestProcessor : SlackRequestProcessor
    {
        private IPlayerCommandService _commandService;
        private IPlayerQueryService _playerQueryService;
        private string _face;
        private string _updatedFace;
        private string _updatedName;
        private string _updatedSlackUsername;
        private const string _facePropertyName = "face";
        private const string _playerNamePropertyName = "name";
        private const string _slackUsernamePropertyName = "slackusername";

        public UpdatePlayerSlackRequestProcessor(IPlayerCommandService commandService, 
            IPlayerQueryService playerQueryService)
        {
            _commandService = commandService;
            _playerQueryService = playerQueryService;
        }

        public override string CommandText
        {
            get
            {
                return "update-player";
            }
        }

        public override ValidationResult ValidateRequest(SlackRequest request)
        {
            SetDataFromCommandText(request.text);

            return ValidationResult.ValidResult("Updating data for ");
        }

        protected override void ExecuteRequest(SlackRequest request)
        {
            var playerId = _playerQueryService.ResolvePlayerId(_face);

            _commandService.UpdatePlayer(new UpdatePlayerCommand
            {
                PlayerId = playerId,
                Face = _updatedFace,
                Name = _updatedName,
                SlackUsername = _updatedSlackUsername,
            });

            SendResponse(request.response_url, string.Format("{0} `updated successfully`", _face));
        }

        private void SetDataFromCommandText(string commandText)
        {
            string[] commandWords = commandText.Split();

            if (commandWords.Length < 3)
                throw new Exception("Invalid Command");

            _face = commandWords[1];

            foreach(var property in commandWords.Skip(2))
            {
                if (string.IsNullOrEmpty(property))
                    continue;
                if (!property.Contains(":"))
                    continue;

                var keyValue = property.Split(':');

                if (keyValue[0] == _playerNamePropertyName)
                    _updatedName = keyValue[1];

                if (keyValue[0] == _facePropertyName)
                    _updatedFace = keyValue[1];

                if (keyValue[0] == _slackUsernamePropertyName)
                    _updatedSlackUsername = keyValue[1];
            }
        }
    }
}