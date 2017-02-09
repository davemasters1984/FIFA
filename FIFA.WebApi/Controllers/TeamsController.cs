using FIFA.CommandServices.Interface;
using FIFA.Model;
using Raven.Client;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace FIFA.WebApi.Controllers
{
    [RoutePrefix("api/teams")]
    public class TeamsController : BaseController
    {
        private ITeamCommandService _commandService;

        public TeamsController(IDocumentStore documentStore, ITeamCommandService commandService)
            :base(documentStore)
        {
            _commandService = commandService;
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

        [HttpPost]
        [Route("{id:int}/badge")]
        [ResponseType(typeof(string))]
        public IHttpActionResult SetBadge(int id, [FromBody]SetTeamBadgeCommand command)
        {
            command.TeamId = TranslateId<Team>(id);
            _commandService.SetBadge(command);

            return Ok("Badge Set");
        }
    }
}
