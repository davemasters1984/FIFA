using FIFA.CommandServices;
using FIFA.CommandServices.Interface;
using FIFA.Infrastructure;
using FIFA.Infrastructure.IoC;
using FIFA.Model.Services;
using FIFA.QueryServices.Interface;
using FIFA.QueryServices.Services;
using FIFA.WebApi.Infrastructure.Slack;
using FIFA.WebApi.Infrastructure.Slack.Processors;
using Microsoft.Practices.Unity;
using System.Web.Http;
using Unity.WebApi;

namespace FIFA.WebApi
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            RegisterTypes(container);

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);

            UnityHelper.SetContainer(container);
        }

        public static void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<IUnitOfWork, RavenDbUnitOfWork>();
            container.RegisterType<IRepository, RavenRepository>();

            container.RegisterInstance(DocumentStoreFactory.CreateDocumentStore(), new ContainerControlledLifetimeManager());

            container.RegisterType<ILeagueCommandService, LeagueCommandService>();
            container.RegisterType<ITeamCommandService, TeamCommandService>();
            container.RegisterType<IPlayerCommandService, PlayerCommandService>();

            container.RegisterType<ILeagueQueryService, LeagueQueryService>();
            container.RegisterType<IStatisticQueryService, StatisticQueryService>();
            container.RegisterType<IPlayerQueryService, PlayerQueryService>();

            container.RegisterType<ILeagueService, LeagueService>();
            container.RegisterType<IResultService, ResultService>();

            container.RegisterType<ISlackRequestService, SlackRequestService>();
            container.RegisterType<ISlackRequestProcessor, GetLeagueSlackRequestProcessor>("league");
            container.RegisterType<ISlackRequestProcessor, PostResultSlackRequestProcessor>("result");
            container.RegisterType<ISlackRequestProcessor, GetFixturesSlackRequestProcessor>("fixtures");
            container.RegisterType<ISlackRequestProcessor, GetResultsSlackRequestProcessor>("results");
            container.RegisterType<ISlackRequestProcessor, GetFormTableSlackRequestProcessor>("form");
            container.RegisterType<ISlackRequestProcessor, GetPlayerPositionChartSlackRequestProcessor>("chart");
            container.RegisterType<ISlackRequestProcessor, GetWeeklySummarySlackRequestProcessor>("week");
            container.RegisterType<ISlackRequestProcessor, GetTopGoalScorersSlackProcessors>("golden-boot");
            container.RegisterType<ISlackRequestProcessor, GetPredictedLeagueTableSlackRequestProcessor>("predicted");
            container.RegisterType<ISlackRequestProcessor, GetHeadToHeadForLeagueSlackRequestProcessor>("h2h");
            container.RegisterType<ISlackRequestProcessor, GetWhosWhoSlackRequestProcessor>("whos-who");
            container.RegisterType<ISlackRequestProcessor, UpdatePlayerSlackRequestProcessor>("update-player");
            container.RegisterType<ISlackRequestProcessor, CreateNewLeagueSlackRequestProcessor>("create-league");
        }
    }
}