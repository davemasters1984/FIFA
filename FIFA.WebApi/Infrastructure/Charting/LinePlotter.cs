using FIFA.QueryServices.Interface.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FIFA.WebApi.Infrastructure.Charting
{
    public class LinePlotter
    {
        private string _playerName;
        private List<PlayerPosition> _positions;
        private readonly double _xAxisIncrementAmount;
        private readonly double _yAxisIncrementAmount;
        private Point _currentPoint = new Point(50, 750);
        private Graphics _graphics;
        private bool _isFirstResult = true;
        private Color _color;

        public LinePlotter(Graphics graphics, Color color, string playerName, IEnumerable<PlayerPosition> positions)
        {
            _positions = positions.ToList();
            _playerName = playerName;
            _graphics = graphics;
            _color = color;

            _xAxisIncrementAmount = 1600 / positions.Count();
            _yAxisIncrementAmount = 750 / 18;
        }

        public void Plot()
        {
            foreach (var position in _positions)
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