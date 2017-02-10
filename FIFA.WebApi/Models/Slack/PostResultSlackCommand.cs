using FIFA.Infrastructure.IoC;
using Raven.Client;
using Microsoft.Practices.Unity;
using System;
using FIFA.Model;
using System.Linq;
using FIFA.CommandServices.Interface;

namespace FIFA.WebApi.Models.Slack
{
    public class PostResultSlackCommand : SlackCommand
    {
        private string _homePlayerFace;
        private int _homeGoals;
        private string _awayPlayerFace;
        private int _awayGoals;

        public override string CommandText
        {
            get
            {
                return "result";
            }
        }

        public override string Execute(SlackRequest request)
        {
            SetDataFromCommandText(request.text);

            var documentStore = UnityHelper.Container.Resolve<IDocumentStore>();
            var leagueService = UnityHelper.Container.Resolve<ILeagueCommandService>();

            Player homePlayer;
            Player awayPlayer;
            League league;

            using (var session = documentStore.OpenSession())
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

            leagueService.PostResult(new CommandServices.Interface.PostResultCommand
            {
                LeagueId = league.Id,
                HomePlayerId = homePlayer.Id,
                AwayPlayerId = awayPlayer.Id,
                HomePlayerGoals = _homeGoals,
                AwayPlayerGoals = _awayGoals,
            });

            return "Result added successfully";
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
    }
}