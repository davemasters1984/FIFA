using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FIFA.WebApi.Infrastructure.Charting
{
    public class DateAxisDrawer
    {
        private List<DateTime> _dates;
        private Graphics _graphics;
        private Color _color;
        private double _xAxisIncrementAmount;
        private StringFormat _drawFormat = new StringFormat();
        private Point _currentPoint = new Point(50, 750);
        private bool _isFirst = true;

        public DateAxisDrawer(Graphics graphics, Color color, IEnumerable<DateTime> dates)
        {
            _graphics = graphics;
            _color = color;
            _dates = dates.ToList();
            _drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            _xAxisIncrementAmount = 1600 / dates.Count();
        }

        public void Draw()
        {
            foreach (var date in _dates)
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
}