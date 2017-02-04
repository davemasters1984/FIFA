using FIFA.Model;
using FIFA.Model.Assigners;
using FIFA.WebApi.Helpers;
using FIFA.WebApi.Infrastructure;
using FIFA.WebApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace FIFA.WebApi.Controllers
{
    [RoutePrefix("api/leagues")]
    public class LeaguesController : BaseController
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
            var key = DocumentStore.Conventions.FindFullDocumentKeyFromNonStringIdentifier(id, typeof(League), false);

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
            var helper = new CreateLeagueHelper();

            var args = helper.CreateLeagueArgs(_participantNames);

            var leagueService = new LeagueService();

            var league = leagueService.CreateNewLeague(args);

            using (var session = DocumentStore.OpenSession())
            {
                session.Store(league);
                session.SaveChanges();
            }

            return Ok(league);
        }

        [HttpPost]
        [Route("{id:int}/results")]
        [ResponseType(typeof(string))]
        public IHttpActionResult PostLeagueResult(int id, [FromBody] Result result)
        {
            result.LeagueId = DocumentStore.Conventions.FindFullDocumentKeyFromNonStringIdentifier(id, typeof(League), false);
            result.AwayPlayerId = DocumentStore.Conventions.FindFullDocumentKeyFromNonStringIdentifier(result.AwayPlayerId, typeof(Player), false);
            result.HomePlayerId = DocumentStore.Conventions.FindFullDocumentKeyFromNonStringIdentifier(result.HomePlayerId, typeof(Player), false);

            using (var session = DocumentStore.OpenSession())
            {
                session.Store(result);

                var league = session.Load<League>(result.LeagueId);

                if (league.ResultIds == null)
                    league.ResultIds = new List<string>();

                league.ResultIds.Add(result.Id);

                session.Store(league);
                session.SaveChanges();
            }

            return Ok("Result posted successfully");
        }
    }

    
}
