using System.Drawing;
using System.Linq;

namespace FIFA.WebApi.Infrastructure.Charting
{
    public class PositionAxisDrawer
    {
        private int _positions;
        private double _yAxisIncrementAmount;
        private Graphics _graphics;
        private Color _color;
        private Point _currentPointLeft = new Point(40, 738);
        private Point _currentPointRight = new Point(50, 738);
        private int _currentPosition;

        public PositionAxisDrawer(Graphics graphics, Color color, int positions)
        {
            _graphics = graphics;
            _color = color;
            _positions = positions;
            _yAxisIncrementAmount = 750 / positions;
            _currentPosition = positions;
        }

        public void DrawAxis()
        {
            for (int position = 0; position < _positions; position++)
                DrawerNotch();
        }

        private void DrawerNotch()
        {
            _graphics.DrawLine(new Pen(_color, 4), _currentPointLeft, _currentPointRight);
            _graphics.DrawLine(new Pen(Color.LightGray, 1), _currentPointLeft, new Point(_currentPointRight.X + 1600, _currentPointRight.Y));

            _graphics.DrawString(_currentPosition.ToString(),
                new Font(FontFamily.Families.Where(f => f.Name.ToLower().Contains("arial")).First(), 10),
                new Pen(_color, 3).Brush,
                new PointF(_currentPointLeft.X - 20, _currentPointLeft.Y - 7)
            );

            _currentPointLeft = new Point(_currentPointLeft.X, _currentPointLeft.Y - (int)_yAxisIncrementAmount);
            _currentPointRight = new Point(_currentPointRight.X, _currentPointRight.Y - (int)_yAxisIncrementAmount);
            _currentPosition--;

        }


    }
}