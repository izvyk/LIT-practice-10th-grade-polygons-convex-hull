using System;
using System.Drawing;

namespace Figures
{
    [Serializable]
    public class Triangle : Vertex
    {
        public Triangle(in int X, in int Y) : base(X, Y) { }

        public override void Draw(in Graphics e, in bool DrawBorder, in Color VertexColor, in Color LineColor, in int LineWidth, in int R, in int DeltaX = 0, in int DeltaY = 0)
        {
            Point[] Points = new Point[]
            {
                new Point((int)(X - Math.Sqrt(3) * R / 2 + DeltaX), Y + R / 2 + DeltaY), // left
                new Point(X + DeltaX, Y - R + DeltaY), // highest
                new Point((int)(X + Math.Sqrt(3) * R / 2 + DeltaX), Y + R / 2 + DeltaY)
            };

            e.FillPolygon(new SolidBrush(VertexColor), Points); //right
            if (DrawBorder) e.DrawPolygon(new Pen(LineColor, LineWidth), Points);
        }

        public override bool Check(in int X, in int Y, in int R) => Check(new Point(X, Y), R);

        public override bool Check(in Point P, in int R)
        {
            bool Vector(in Point K, in Point L, in Point M) // вектор AC справа от вектора AB или лежит на нём?
            {
                return (L.X - K.X) * (M.Y - K.Y) - (M.X - K.X) * (L.Y - K.Y) >= 0;
            }

            Point A = new Point((int)(X - Math.Sqrt(3) * R / 2), Y + R / 2), // left
                  B = new Point(X, Y - R), // highest
                  C = new Point((int)(X + Math.Sqrt(3) * R / 2), Y + R / 2); // right
            return Vector(A, B, P) && Vector(B, C, P) && Vector(C, A, P);
        }
    }
}
