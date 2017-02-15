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
            using (var graphics = Graphics.FromImage(img))
            {
                // Use the Graphics object to modify it

                var playerOnePlotter = new LinePlotter(graphics, Color.Green, playerComparison.PlayerOneName, playerComparison.PlayerOnePositionHistory);
                var playerTwoPlotter = new LinePlotter(graphics, Color.Blue, playerComparison.PlayerTwoName, playerComparison.PlayerTwoPositionHistory);

                playerOnePlotter.Plot();
                playerTwoPlotter.Plot();

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

        private class LinePlotter
        {
            private string _playerName;
            private List<PlayerPosition> _positions;
            private readonly double _xAxisIncrementAmount;
            private readonly double _yAxisIncrementAmount;
            private Point _currentPoint = new Point(21, 479);
            private Graphics _graphics;
            private bool _isFirstResult = true;
            private Color _color;

            public LinePlotter(Graphics graphics, Color color, string playerName, IEnumerable<PlayerPosition> positions)
            {
                _positions = positions.ToList();
                _playerName = playerName;
                _graphics = graphics;
                _color = color;

                _xAxisIncrementAmount = 479 / positions.Count();
                _yAxisIncrementAmount = 379 / 18;

            }

            public void Plot()
            {
                foreach(var position in _positions)
                    PlotNext(position);

                RenderPlayerName();
            }



            private void PlotNext(PlayerPosition position)
            {
                var newYPosition = _yAxisIncrementAmount * position.Position;

                var newXPosition = (_isFirstResult) 
                    ? _currentPoint.X 
                    : _currentPoint.X + _xAxisIncrementAmount;

                var newPoint = new Point((int)newXPosition, (int)newYPosition);

                if (!_isFirstResult)
                {
                    DrawLine(_currentPoint, newPoint);
                }

                _currentPoint = newPoint;
                _isFirstResult = false;
            }

            private void RenderPositionNumber(PlayerPosition position)
            {
                if (position != _positions[_positions.Count - 1])
                    return;

                _graphics.DrawString(position.Position.ToString(),
                    new Font(FontFamily.Families.Where(f => f.Name.ToLower().Contains("arial")).First(), 10),
                    new Pen(_color, 3).Brush,
                    new PointF(_currentPoint.X - 10, _currentPoint.Y + 5)
                );
            }

            private void RenderPlayerName()
            {
                _graphics.DrawString(_playerName,
                    new Font(FontFamily.Families.Where(f => f.Name.ToLower().Contains("arial")).First(), 10),
                    new Pen(_color, 3).Brush,
                    new PointF(_currentPoint.X - 10, _currentPoint.Y + 5)
                );
            }

            private void DrawLine(Point startPoint, Point endPoint)
            {
                _graphics.DrawLine(new Pen(_color, 4), startPoint, endPoint);
            }
        }
    }

    
}
