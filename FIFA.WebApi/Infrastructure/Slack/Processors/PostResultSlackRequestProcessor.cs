using FIFA.CommandServices.Interface;
using FIFA.Model;
using FIFA.QueryServices.Interface;
using FIFA.WebApi.Models.Slack;
using Raven.Client;
using System;
using System.Linq;

namespace FIFA.WebApi.Infrastructure.Slack
{
    public class PostResultSlackRequestProcessor : SlackRequestProcessor
    {
        private string _homePlayerFace;
        private string _homePlayerId;
        private int _homeGoals;
        private string _awayPlayerFace;
        private string _awayPlayerId;
        private int _awayGoals;
        private IDocumentStore _documentStore;
        private ILeagueCommandService _leagueCommandService;
        private IPlayerQueryService _playerQueryService;

        public override string CommandText
        {
            get
            {
                return "result";
            }
        }

        public PostResultSlackRequestProcessor(IDocumentStore documentStore, 
            IPlayerQueryService playerQueryService,
            ILeagueCommandService leagueCommandService)
        {
            _documentStore = documentStore;
            _leagueCommandService = leagueCommandService;
            _playerQueryService = playerQueryService;
        }

        protected override void ExecuteRequest(SlackRequest request)
        {
            League league;

            using (var session = _documentStore.OpenSession())
            {
                league = session.Query<League>()
                    .OrderByDescending(l => l.CreatedDate)
                    .FirstOrDefault();
            }

            _leagueCommandService.PostResult(new PostResultCommand
            {
                LeagueId = league.Id,
                HomePlayerId = _homePlayerId,
                AwayPlayerId = _awayPlayerId,
                HomePlayerGoals = _homeGoals,
                AwayPlayerGoals = _awayGoals,
            });

            SendResponse(request.response_url, string.Format("{0} {1} - {2} {3} `added successfully`", _homePlayerFace, _homeGoals, _awayGoals, _awayPlayerFace));
        }

        private void SetDataFromCommandText(string commandText)
        {
            //result :dave: 1 - 0 :ash:

            string[] commandWords = commandText.Split();

            if (commandWords.Length < 6)
                throw new Exception(string.Format("Could not understand command: '{0}'. Results should be in the format 'result :face: 1 - 1 :face:'", commandText));

            _homePlayerFace = commandWords[1];
                
            if (!int.TryParse(commandWords[2], out _homeGoals))
                throw new Exception(string.Format("Invalid home goals: '{0}'", commandWords[2]));

            if (!int.TryParse(commandWords[4], out _awayGoals))
                throw new Exception(string.Format("Invalid away goals: '{0}'", commandWords[4]));

            _awayPlayerFace = commandWords[5];
        }

        private void ResolvePlayerIds()
        {
            _homePlayerId = _playerQueryService.ResolvePlayerId(_homePlayerFace);
            _awayPlayerId = _playerQueryService.ResolvePlayerId(_awayPlayerFace);
        }

        public override ValidationResult ValidateRequest(SlackRequest request)
        {
            try
            {
                SetDataFromCommandText(request.text);
                ResolvePlayerIds();

                return ValidationResult.ValidResult("`Adding result into league`");
            }
            catch (Exception ex)
            {
                return ValidationResult.InvalidResult(string.Format("`Unable to add result: {0}`", ex.Message));
            }
        }
    }
}