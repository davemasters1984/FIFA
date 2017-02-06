using FIFA.CommandServices.Interface;
using FIFA.Model;
using FIFA.WebApi.Infrastructure;
using FIFA.WebApi.Models;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace FIFA.WebApi.Controllers
{
    [RoutePrefix("api/leagues")]
    public class LeaguesController : BaseController
    {
        private ILeagueCommandService _leagueCommandService;

        public LeaguesController(ILeagueCommandService leagueCommandService)
        {
            _leagueCommandService = leagueCommandService;
        }

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

        [Route("")]
        public IHttpActionResult Get()
        {
            using (var session = DocumentStore.OpenSession())
            {
                var leagues
                    = session.Query<League>()
                        .Select(l => new LeagueSummary
                        {
                            Id = l.Id, 
                            NumberOfParticipants = l.Participants.Count,
                            CreatedDate = l.CreatedDate,
                        })
                        .ToList();

                return Ok(leagues);
            }
        }

        [Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            var key = DocumentStore.Conventions.FindFullDocumentKeyFromNonStringIdentifier(id, typeof(League), false);

            using (var session = DocumentStore.OpenSession())
            {
                var leagueTable
                    = session.Query<LeagueTableRow, LeagueTableIndex>()
                        .Where(l => l.LeagueId == key)
                        .ToList();

                return Ok(leagueTable);
            }
        }

        [Route("{id:int}/results")]
        [ResponseType(typeof(string))]
        public IHttpActionResult GetLeagueResults(int id)
        {
            var key = TranslateId<League>(id);

            using (var session = DocumentStore.OpenSession())
            {
                var leagueTable
                    = session.Query<ResultSummary, ResultsIndex>()
                        .Where(l => l.LeagueId == key)
                        .ToList();

                return Ok(leagueTable);
            }
        }

        [Route("{id:int}/form")]
        [ResponseType(typeof(string))]
        public IHttpActionResult GetLeagueForm(int id)
        {
            return Ok("This is the league form table");
        }

        [Route("{id:int}/fixtures")]
        [ResponseType(typeof(string))]
        public IHttpActionResult GetLeagueFixtures(int id)
        {
            return Ok("This is the remaining fixtures");
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Create()
        {
            _leagueCommandService.CreateLeague(new CreateLeagueCommand
            {
                ParticipantFaces = _participantNames
            });

            return Ok("League Created Successfully");
        }

        [HttpPost]
        [Route("{id:int}/results")]
        [ResponseType(typeof(string))]
        public IHttpActionResult PostLeagueResult(int id, [FromBody] PostResultCommand command)
        {
            command.LeagueId = TranslateId<League>(id);
            command.AwayPlayerId = TranslateId<Player>(command.AwayPlayerId);
            command.HomePlayerId = TranslateId<Player>(command.HomePlayerId);

            _leagueCommandService.PostResult(command);

            return Ok("Result posted successfully");
        }
    }

    
}
