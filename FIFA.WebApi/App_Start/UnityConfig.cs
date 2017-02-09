using FIFA.CommandServices;
using FIFA.CommandServices.Interface;
using FIFA.Infrastructure;
using FIFA.Infrastructure.IoC;
using FIFA.Model.Services;
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

            container.RegisterType<ILeagueService, LeagueService>();
            container.RegisterType<IResultService, ResultService>();
        }
    }
}