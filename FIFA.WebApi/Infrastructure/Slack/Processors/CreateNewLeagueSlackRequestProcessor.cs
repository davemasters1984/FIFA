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
    public class CreateNewLeagueSlackRequestProcessor : SlackRequestProcessor
    {
        private ILeagueCommandService _commandService;
        private string _leagueName;
        private decimal _maxRating;
        private decimal _minRating;
        private List<string> _faces;
        private ILeagueQueryService _leagueQueryService;

        public CreateNewLeagueSlackRequestProcessor(ILeagueCommandService commandService, 
            ILeagueQueryService leagueQueryService)
        {
            _commandService = commandService;
            _leagueQueryService = leagueQueryService;
        }

        public override string CommandText
        {
            get
            {
                return "create-league";
            }
        }

        public override string Description
        {
            get
            {
                return "Creates a new league";
            }
        }

        public override string ExampleRequest
        {
            get
            {
                return $"{SlackSlashCommand} {CommandText} premiership 3.5-5 :dave: :dom: :matt: :liam: :ash:";
            }
        }

        public override ValidationResult ValidateRequest(SlackRequest request)
        {
            try
            {
                SetDataFromCommandText(request.text);

                return ValidationResult.ValidResult("`Creating league, please wait...`");
            }
            catch (Exception ex)
            {
                return ValidationResult.InvalidResult(ex.Message);
            }
        }

        private void SetDataFromCommandText(string commandText)
        {
            string[] commandWords = commandText.Split();

            if (commandWords.Length < 3)
                throw new Exception("Invalid Command");

            _leagueName = commandWords[1];

            ParseTeamRatingLimits(commandWords[2]);

            _faces = new List<string>();

            for (int i = 3; i < commandWords.Length; i++)
                _faces.Add(commandWords[i].Trim());

        }

        private void ParseTeamRatingLimits(string ratingsCommandText)
        {
            var ratings = ratingsCommandText.Split('-');

            if (!decimal.TryParse(ratings[0], out _minRating))
                throw new Exception("Could not parse minimum team rating as a decimal");

            if (!decimal.TryParse(ratings[1], out _maxRating))
                throw new Exception("Could not parse minimum team rating as a decimal");
        }

        protected override void ExecuteRequest(SlackRequest request)
        {
            _commandService.CreateLeague(new CreateLeagueCommand
            {
                Name = _leagueName,
                MinimumTeamRating = _minRating,
                MaximumTeamRating = _maxRating,
                ParticipantFaces = _faces
            });

            var leagueId = _leagueQueryService.GetCurrentLeagueIdFromLeagueName(_leagueName);
            var league = _leagueQueryService.GetLeagueTable(leagueId);

            var responseBuilder = new StringBuilder();

            foreach (var row in league.OrderByDescending(r => r.TeamRating))
            {
                responseBuilder.AppendFormat("\n{0} *{1}* is {2} *{3}* `[{4}]`",
                    row.PlayerFace,
                    row.PlayerName,
                    row.TeamBadge,
                    row.TeamName,
                    row.TeamRating);
            }

            SendResponse(request.response_url, responseBuilder.ToString());
        }
    }
}