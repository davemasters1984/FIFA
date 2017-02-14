using FIFA.CommandServices.Interface;
using FIFA.Model;
using FIFA.QueryServices.Indexes;
using FIFA.QueryServices.Interface.Models;
using FIFA.QueryServices.Models;
using Raven.Client;
using System.Drawing;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web;
using System.IO;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System;
using FIFA.QueryServices.Interface;

namespace FIFA.WebApi.Controllers
{
    [RoutePrefix("api/leagues")]
    public class LeaguesController : BaseController
    {
        private ILeagueCommandService _leagueCommandService;
        private ILeagueQueryService _leagueQueryService;

        public LeaguesController(ILeagueCommandService leagueCommandService, 
            ILeagueQueryService leagueQueryService,
            IDocumentStore documentStore)
            :base(documentStore)
        {
            _leagueCommandService = leagueCommandService;
            _leagueQueryService = leagueQueryService;
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
            ":luke:",
            ":carl:",
            ":rich:",
            ":jonny:"
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

        [Route("{leagueId:int}/players/{playerOneId:int}/compare/{playerTwoId:int}")]
        public HttpResponseMessage GetPlayersGraph(int leagueId, int playerOneId, int playerTwoId)
        {
            var playerOneIdString = TranslateId<Player>(playerOneId);
            var playerTwoIdString = TranslateId<Player>(playerTwoId);

            var playerComparison 
                = _leagueQueryService.GetPlayerPositionHistoryComparisonForCurrentLeague(playerOneIdString, playerTwoIdString);

            using (var img = Image.FromFile(HttpContext.Current.Server.MapPath("~/App_Data/empty-line-graph.png")))
            using (var g = Graphics.FromImage(img))
            {
                // Use the Graphics object to modify it
                g.DrawLine(new Pen(Color.Red), new Point(0, 0), new Point(50, 50));
                g.DrawString(playerComparison.PlayerOneName,
                    new Font(FontFamily.Families.Where(f => f.Name.ToLower().Contains("arial")).First(), 20),
                    new Pen(Color.White, 2).Brush,
                    new PointF(30, 100)
                );

                g.DrawString(playerComparison.PlayerTwoName,
                      new Font(FontFamily.Families.Where(f => f.Name.ToLower().Contains("arial")).First(), 20),
                      new Pen(Color.White, 2).Brush,
                      new PointF(100, 300)
                  );

                // Write the resulting image to the response stream
                using (var stream = new MemoryStream())
                {
                    img.Save(stream, ImageFormat.Png);

                    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                    result.Content = new ByteArrayContent(stream.ToArray());
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    return result;
                }
            }
        }
    }

    
}
