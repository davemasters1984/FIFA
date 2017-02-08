using FIFA.Model;
using Raven.Client;
using System.Linq;
using System.Web.Http;

namespace FIFA.WebApi.Controllers
{
    public class PlayersController : BaseController
    {
        public PlayersController(IDocumentStore documentStore)
            :base(documentStore)
        {

        }

        public IHttpActionResult Get()
        {
            using (var session = DocumentStore.OpenSession())
            {
                var players = session.Query<Player>()
                    .ToList();

                return Ok(players);
            }
        }
    }
}