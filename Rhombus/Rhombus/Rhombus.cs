using System;
using System.Drawing;

namespace Figures
{
    [Serializable]
    public class Rhombus : Vertex
    {
        public Rhombus(in int x, in int y) : base(x, y) { }

        public override void Draw(in Graphics e, in bool drawBorder, in Color vertexColor, in Color lineColor, in int lineWidth, in int r, in int deltaX = 0, in int deltaY = 0)
        {
            Point[] points =
            {
                new Point((X + deltaX), Y - r + deltaY), // up
                new Point((X + r + deltaX), Y + deltaY), // right
                new Point(X + deltaX, Y + r + deltaY), // down
                new Point((X - r + deltaX), Y + deltaY) // left
            };

            e.FillPolygon(new SolidBrush(vertexColor), points); //right
            if (drawBorder) e.DrawPolygon(new Pen(lineColor, lineWidth), points);
        }

        public override bool Check(in int x, in int y, in int r) => Check(new Point(x, y), r);

        public override bool Check(in Point point, in int r)
        {
            bool Vector(in Point K, in Point L, in Point M)
            {
                return (L.X - K.X) * (M.Y - K.Y) - (M.X - K.X) * (L.Y - K.Y) >= 0;
            }

            Point a = new Point(X, Y - r), // up
                b = new Point((X + r), Y), // right
                c = new Point(X, Y + r), // down
                d = new Point((X - r), Y); // left
            return Vector(a, b, point) && Vector(b, c, point) && Vector(c, d, point) && Vector(d, a, point);
        }
    }
}
