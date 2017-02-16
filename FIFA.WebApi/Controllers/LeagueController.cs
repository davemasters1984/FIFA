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

        private class PositionAxisDrawer
        {
            private int _positions;
            private double _yAxisIncrementAmount;
            private Graphics _graphics;
            private Color _color;
            private Point _currentPointLeft = new Point(21, 479);
            private Point _currentPointRight = new Point(26, 479);

            public PositionAxisDrawer(Graphics graphics, Color color, int positions)
            {
                _graphics = graphics;
                _color = color;
                _positions = positions;
                _yAxisIncrementAmount = 479 / positions;
            }

            public void DrawAxis()
            {
                for (int i = 1; i < _positions; i++)
                    DrawerNotch();
            }

            private void DrawerNotch()
            {
                _graphics.DrawLine(new Pen(_color, 4), _currentPointLeft, _currentPointRight);

                _currentPointLeft = new Point(_currentPointLeft.X, _currentPointLeft.Y - (int)_yAxisIncrementAmount);
                _currentPointRight = new Point(_currentPointRight.X, _currentPointRight.Y - (int)_yAxisIncrementAmount);
            }
            

        }

        private class DateAxisDrawer
        {
            private List<DateTime> _dates;
            private Graphics _graphics;
            private Color _color;
            private double _xAxisIncrementAmount;
            private StringFormat _drawFormat = new StringFormat();
            private Point _currentPoint = new Point(21, 425);
            private bool _isFirst = true;

            public DateAxisDrawer(Graphics graphics, Color color, IEnumerable<DateTime> dates)
            {
                _graphics = graphics;
                _color = color;
                _dates = dates.ToList();
                _drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
                _xAxisIncrementAmount = 379 / dates.Count();
            }

            public void Draw()
            {
                foreach(var date in _dates)
                    RenderDate(date);
            }

            private void RenderDate(DateTime date)
            {
                if (!_isFirst)
                {
                    _graphics.DrawString(date.ToString("dd MMM"),
                        new Font(FontFamily.Families.Where(f => f.Name.ToLower().Contains("arial")).First(), 10),
                        new Pen(_color, 3).Brush,
                        new PointF(_currentPoint.X - 10, _currentPoint.Y + 5),
                        _drawFormat
                    );
                }

                _currentPoint = new Point(_currentPoint.X + (int)_xAxisIncrementAmount, _currentPoint.Y);
                _isFirst = false;
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

                _xAxisIncrementAmount = 379 / positions.Count();
                _yAxisIncrementAmount = 479 / 18;
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
