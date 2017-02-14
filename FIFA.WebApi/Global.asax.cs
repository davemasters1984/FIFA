using FIFA.QueryServices.Indexes;
using Raven.Client.Document;
using System.Web.Http;

namespace FIFA.WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            UnityConfig.RegisterComponents();

            DeployIndexes();
        }

        protected void DeployIndexes()
        {
            var documentStore = new DocumentStore
            {
                ConnectionStringName = "RavenHQ",
                DefaultDatabase = "FIFA",
            };

            documentStore.Initialize();

            new LeagueTableIndex().Execute(documentStore);
            new ResultsIndex().Execute(documentStore);
            new FixturesIndex().Execute(documentStore);
            new LeagueSummaryIndex().Execute(documentStore);
            new FormTableIndex().Execute(documentStore);
        }
    }
}
