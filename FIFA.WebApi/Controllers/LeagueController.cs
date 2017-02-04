using FIFA.WebApi.Helpers;
using System.Web.Http;

namespace FIFA.WebApi.Controllers
{
    public class LeagueController : ApiController
    {
        private string[] _participantNames = new string[]
        {
            ":neil:",
            ":daveb:",
            ":mattw:",
            ":tristan:",
            ":dom:",
            ":matt:",
            ":liam:",
            ":james:",
            ":louie:",
            ":dave:",
            ":craig:",
            ":ash:",
            ":jakub:",
            ":mogg:",
            ":luke:"
        };

        public IHttpActionResult Get()
        {
            var repo = new LeagueRepository();

            var league = repo.GetCurrentLeague();

            return Ok(league);
        }

        [HttpPost]
        public IHttpActionResult Create()
        {
            var leagueGenerator = new LeagueGenerator();

            var league = leagueGenerator.Generate(_participantNames);

            return Ok(league);
        }
    }
}
