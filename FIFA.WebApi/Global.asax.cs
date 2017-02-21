using FIFA.Infrastructure;
using FIFA.Infrastructure.IoC;
using FIFA.QueryServices.Indexes;
using Raven.Client;
using Raven.Client.Document;
using System.Web.Http;
using Microsoft.Practices.Unity;

namespace FIFA.WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            UnityConfig.RegisterComponents();
        }
    }
}
