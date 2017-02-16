using FIFA.CommandServices.Interface;
using FIFA.Model;
using FIFA.WebApi.Models.Slack;
using Raven.Client;
using System;
using System.Linq;

namespace FIFA.WebApi.Infrastructure.Slack
{
    public class PostResultSlackRequestProcessor : SlackRequestProcessor
    {
        private string _homePlayerFace;
        private int _homeGoals;
        private string _awayPlayerFace;
        private int _awayGoals;
        private IDocumentStore _documentStore;
        private ILeagueCommandService _leagueCommandService;

        public override string CommandText
        {
            get
            {
                return "result";
            }
        }

        public PostResultSlackRequestProcessor(IDocumentStore documentStore, 
            ILeagueCommandService leagueCommandService)
        {
            _documentStore = documentStore;
            _leagueCommandService = leagueCommandService;
        }

        public override void Execute(SlackRequest request)
        {
            SetDataFromCommandText(request.text);

            Player homePlayer;
            Player awayPlayer;
            League league;

            using (var session = _documentStore.OpenSession())
            {
                league = session.Query<League>()
                    .OrderByDescending(l => l.CreatedDate)
                    .FirstOrDefault();

                homePlayer = session.Query<Player>()
                    .Where(p => p.Face == _homePlayerFace)
                    .FirstOrDefault();

                awayPlayer = session.Query<Player>()
                    .Where(p => p.Face == _awayPlayerFace)
                    .FirstOrDefault();
            }

            _leagueCommandService.PostResult(new PostResultCommand
            {
                LeagueId = league.Id,
                HomePlayerId = homePlayer.Id,
                AwayPlayerId = awayPlayer.Id,
                HomePlayerGoals = _homeGoals,
                AwayPlayerGoals = _awayGoals,
            });

            SendResponse(request.response_url, string.Format("{0} {1} - {2} {3} `added successfully`", _homePlayerFace, _homeGoals, _awayGoals, _awayPlayerFace));
        }

        private void SetDataFromCommandText(string commandText)
        {
            //fifaleague result :dave: 1 - 0 :ash:

            string[] commandWords = commandText.Split();

            if (commandText.Length < 6)
                throw new Exception("Invalid Command");

            _homePlayerFace = commandWords[1];
            _homeGoals = int.Parse(commandWords[2]);
            _awayGoals = int.Parse(commandWords[4]);
            _awayPlayerFace = commandWords[5];
        }

        public override ValidationResult ValidateRequest(SlackRequest request)
        {
            return ValidationResult.ValidResult("`Adding result into league`");
        }
    }
}