using Raven.Client;
using System.Web.Http;

namespace FIFA.WebApi.Controllers
{
    public class BaseController : ApiController
    {
        protected IDocumentStore DocumentStore;

        protected BaseController(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }

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

        }
    }
}
