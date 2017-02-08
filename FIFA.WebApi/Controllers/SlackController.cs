using FIFA.Model;
using FIFA.QueryServices.Indexes;
using FIFA.QueryServices.Models;
using Raven.Client;
using Raven.Client.Linq;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace FIFA.WebApi.Controllers
{
    [RoutePrefix("api/slack")]
    public class SlackController : BaseController
    {
        public SlackController(IDocumentStore documentStore)
            :base(documentStore)
        {

        }

        [Route("league/current")]
        public HttpResponseMessage GetCurrentLeague()
        {
            using (var session = DocumentStore.OpenSession())
            {
                var latestLeague = session.Query<League>()
                    .OrderByDescending(l => l.CreatedDate)
                    .FirstOrDefault();

                var leagueTable
                    = session.Query<LeagueTableRow, LeagueTableIndex>()
                        .Where(l => l.LeagueId == latestLeague.Id)
                        .OrderBy(l => l.Points)
                        .ToList();

                var response = new StringBuilder();

                foreach (var row in leagueTable)
                {
                    response.AppendFormat(string.Format("\n{0} {1} {2}",
                        row.PlayerFace,
                        row.PlayerName,
                        row.TeamName,
                        row.GamesPlayed,
                        row.Points));
                }

                var resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Content = new StringContent(response.ToString(), Encoding.UTF8, "text/plain");

                return resp;

            }
        }
    }
}