using System;
using System.Drawing;

namespace Figures
{
    [Serializable]
    public class Triangle : Vertex
    {
        public Triangle(in int x, in int y) : base(x, y) { }

        public override void Draw(in Graphics e, in bool drawBorder, in Color vertexColor, in Color lineColor, in int lineWidth, in int r, in int deltaX = 0, in int deltaY = 0)
        {
            Point[] points =
            {
                new Point((int)(X - Math.Sqrt(3) * r / 2 + deltaX), Y + r / 2 + deltaY), // left
                new Point(X + deltaX, Y - r + deltaY), // highest
                new Point((int)(X + Math.Sqrt(3) * r / 2 + deltaX), Y + r / 2 + deltaY)
            };

            e.FillPolygon(new SolidBrush(vertexColor), points); //right
            if (drawBorder) e.DrawPolygon(new Pen(lineColor, lineWidth), points);
        }

        public override bool Check(in int x, in int y, in int r) => Check(new Point(x, y), r);

        public override bool Check(in Point p, in int r)
        {
            bool Vector(in Point k, in Point l, in Point m)
            {
                return (l.X - k.X) * (m.Y - k.Y) - (m.X - k.X) * (l.Y - k.Y) >= 0;
            }

            Point a = new Point((int)(X - Math.Sqrt(3) * r / 2), Y + r / 2), // left
                  b = new Point(X, Y - r), // highest
                  c = new Point((int)(X + Math.Sqrt(3) * r / 2), Y + r / 2); // right
            return Vector(a, b, p) && Vector(b, c, p) && Vector(c, a, p);
        }
    }
}
