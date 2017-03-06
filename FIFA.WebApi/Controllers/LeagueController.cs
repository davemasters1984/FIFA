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
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using FIFA.WebApi.Infrastructure.Charting;

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

        [Route("{leagueId:int}/position-history/{playerIds}")]
        public HttpResponseMessage GetPlayerPostitionHistoryChart(int leagueId, string playerIds)
        {
            var translatedIds = GetTranslatedPlayerIds(playerIds);
            var translatedLeagueId = TranslateId<League>(leagueId);

            var positionHistory = 
                _leagueQueryService.GetPostionHistoryForPlayers(translatedLeagueId, translatedIds);

            using (var img = Image.FromFile(HttpContext.Current.Server.MapPath("~/App_Data/Blank-Chart.png")))
            using (var graphics = CreateGraphicsFromImage(img))
            {
                var dates = GetDistinctDatesFromPlayerHistory(positionHistory);
                var linePlotters = GetPlottersForPlayerHistories(graphics, positionHistory);

                var datesDrawer = new DateAxisDrawer(graphics, Color.Black, dates);
                var positionAxisDrawer = new PositionAxisDrawer(graphics, Color.Black, 18);

                datesDrawer.Draw();
                positionAxisDrawer.DrawAxis();

                foreach (var linePlotter in linePlotters)
                    linePlotter.Plot();

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

        private Graphics CreateGraphicsFromImage(Image image)
        {
            var graphics = Graphics.FromImage(image);

            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            return graphics;
        }

        private Color GetRandomColour(Random random)
        {
            return Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
        }

        private IEnumerable<DateTime> GetDistinctDatesFromPlayerHistory(IEnumerable<PlayerPositionHistory> history)
        {
            return history
                .SelectMany(x => x.History.Select(h => h.Date))
                .Distinct()
                .OrderBy(d => d)
                .ToList();
        }

        private IEnumerable<LinePlotter> GetPlottersForPlayerHistories(Graphics graphics, IEnumerable<PlayerPositionHistory> history)
        {
            var plotters = new List<LinePlotter>();
            var random = new Random();

            foreach(var playerHistory in history)
            {
                var colour = GetRandomColour(random);
                var plotter = new LinePlotter(graphics, colour, playerHistory.PlayerName, playerHistory.History);

                plotters.Add(plotter);
            }

            return plotters;

        }

        private IEnumerable<string> GetTranslatedPlayerIds(string playerIds)
        {
            var playerIdsWithoutPrefix = playerIds.Split(',');

            var translatedIds = new List<string>();

            foreach (var id in playerIdsWithoutPrefix)
                translatedIds.Add(TranslateId<Player>(id));

            return translatedIds;
        }

        [Route("{leagueId:int}/players/{playerOneId:int}/chart/{playerTwoId:int}")]
        public HttpResponseMessage GetPlayersGraph(int leagueId, int playerOneId, int playerTwoId)
        {
            var playerOneIdString = TranslateId<Player>(playerOneId);
            var playerTwoIdString = TranslateId<Player>(playerTwoId);

            var playerComparison 
                = _leagueQueryService.GetPlayerPositionHistoryComparisonForCurrentLeague(playerOneIdString, playerTwoIdString);

            using (var img = Image.FromFile(HttpContext.Current.Server.MapPath("~/App_Data/graph.png")))
            using (var graphics = Graphics.FromImage(img))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // Use the Graphics object to modify it
                var colorOne = ColorTranslator.FromHtml("#3CC1C9");
                var colorTwo = ColorTranslator.FromHtml("#F2DB1C");

                var dates = playerComparison.PlayerOnePositionHistory
                    .Select(d => d.Date)
                    .Union(playerComparison.PlayerTwoPositionHistory
                    .Select(d => d.Date))
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();

                var playerOnePlotter = new LinePlotter(graphics, colorOne, playerComparison.PlayerOneName, playerComparison.PlayerOnePositionHistory);
                var playerTwoPlotter = new LinePlotter(graphics, colorTwo, playerComparison.PlayerTwoName, playerComparison.PlayerTwoPositionHistory);
                var datesDrawer = new DateAxisDrawer(graphics, Color.White, dates);
                var positionAxisDrawer = new PositionAxisDrawer(graphics, Color.White, 18);

                playerOnePlotter.Plot();
                playerTwoPlotter.Plot();
                datesDrawer.Draw();
                //positionAxisDrawer.DrawAxis();

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
