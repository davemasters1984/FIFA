using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FIFA.WebApi.Models.Slack;
using FIFA.CommandServices.Interface;
using FIFA.QueryServices.Interface;
using System.Text;

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
        private decimal _updatedOverallScore;
        private const string _facePropertyName = "face";
        private const string _playerNamePropertyName = "name";
        private const string _slackUsernamePropertyName = "slackusername";
        private const string _overallScorePropertyName = "overallscore";

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

        public override string ExampleRequest
        {
            get
            {
                return $"`{SlackSlashCommand} {CommandText} :dave: slackUsername:@davemasters`";
            }
        }

        public override string Description
        {
            get
            {
                return "Updates a player's fields. Available fields: `slackUsername`, `face`, `name`";
            }
        }

        public override ValidationResult ValidateRequest(SlackRequest request)
        {
            try
            {
                SetDataFromCommandText(request.text);
            }
            catch (Exception ex)
            {
                return ValidationResult.InvalidResult(ex.Message);
            }

            return ValidationResult.ValidResult($"Updating data for {_updatedFace}");
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
                OverallScore = _updatedOverallScore
            });

            var players = _playerQueryService.GetPlayers();
            var stringBuilder = new StringBuilder();

            foreach (var player in players.OrderByDescending(p => p.OverallScore))
                stringBuilder.Append($"\n`{player.Face} {player.Name} [{player.OverallScore}]`");

            var responseString = $"{_face} `updated successfully`\n{stringBuilder.ToString()}";

            SendResponse(request.response_url, responseString);
        }

        private void SetDataFromCommandText(string commandText)
        {
            string[] commandWords = commandText.Split();

            if (commandWords.Length < 3)
                throw new Exception($"Could not understand command: '{commandText}'.");

            _face = commandWords[1];

            foreach(var property in commandWords.Skip(2))
            {
                if (string.IsNullOrEmpty(property))
                    continue;
                if (!property.Contains(":"))
                    continue;

                var keyValue = property.Split(':');

                if (keyValue[0].ToLower() == _playerNamePropertyName)
                    _updatedName = keyValue[1];

                if (keyValue[0].ToLower() == _facePropertyName)
                    _updatedFace = keyValue[1];

                if (keyValue[0].ToLower() == _slackUsernamePropertyName)
                    _updatedSlackUsername = keyValue[1];

                if (keyValue[0].ToLower() == _overallScorePropertyName)
                {
                    if (!decimal.TryParse(keyValue[1], out _updatedOverallScore))
                        throw new Exception("Overall score must be a number");
                }
            }
        }
    }
}