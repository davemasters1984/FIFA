using FIFA.Model;
using Raven.Client;
using System.Linq;
using System.Web.Http;

namespace FIFA.WebApi.Controllers
{
    public class TeamsController : BaseController
    {
        public TeamsController(IDocumentStore documentStore)
            :base(documentStore)
        {

        }

        public IHttpActionResult Get()
        {
            using (var session = DocumentStore.OpenSession())
            {
                var teams = session.Query<Team>()
                    .ToList();

                return Ok(teams);
            }
        }
    }
}
