using Raven.Client;
using Raven.Client.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FIFA.WebApi.Controllers
{
    public class BaseController : ApiController
    {
        protected IDocumentStore DocumentStore;

        protected string TranslateId<T>(int id)
        {
            return DocumentStore.Conventions.FindFullDocumentKeyFromNonStringIdentifier(id, typeof(T), false);
        }

        protected string TranslateId<T>(string id)
        {
            return DocumentStore.Conventions.FindFullDocumentKeyFromNonStringIdentifier(id, typeof(T), false);
        }

        protected BaseController()
        {
            DocumentStore = new DocumentStore
            {
                ConnectionStringName = "RavenHQ",
                DefaultDatabase = "FIFA",
            };

            DocumentStore.Initialize();
        }
    }
}
