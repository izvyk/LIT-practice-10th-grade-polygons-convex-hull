using System.Drawing;

namespace Figures
{
    public class Rhombus : Vertex
    {
        public Rhombus(in int X, in int Y) : base(X, Y) { }

        public override void Draw(in Graphics e, in bool DrawBorder, in Color VertexColor, in Color LineColor, in int LineWidth, in int R, in int DeltaX = 0, in int DeltaY = 0)
        {
            Point[] Points = new Point[]
            {
                new Point((X + DeltaX), Y - R + DeltaY), // up
                new Point((X + R + DeltaX), Y + DeltaY), // right
                new Point(X + DeltaX, Y + R + DeltaY), // down
                new Point((X - R + DeltaX), Y + DeltaY) // left
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

            Point A = new Point(X, Y - R); // up
            Point B = new Point((X + R), Y); // right
            Point C = new Point(X, Y + R); // down
            Point D = new Point((X - R), Y); // left
            return Vector(A, B, P) && Vector(B, C, P) && Vector(C, D, P) && Vector(D, A, P);
        }
    }
}
