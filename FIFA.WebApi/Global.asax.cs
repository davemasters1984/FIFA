using FIFA.WebApi.Infrastructure;
using Raven.Client.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

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
            new LeagueSummaryIndex().Execute(documentStore);
        }
    }
}
