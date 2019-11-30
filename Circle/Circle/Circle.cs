using System;
using System.Drawing;

namespace Figures
{
    [Serializable]
    public class Circle : Vertex
    {
        public Circle(in int X, in int Y) : base(X, Y) { }

        public override void Draw(in Graphics e, in bool DrawBorder, in Color VertexColor, in Color LineColor, in int LineWidth, in int R, in int DeltaX = 0, in int DeltaY = 0)
        {
            e.FillEllipse(new SolidBrush(VertexColor), new Rectangle(X - R + DeltaX, Y - R + DeltaY, R * 2, R * 2));
            if (DrawBorder) e.DrawEllipse(new Pen(LineColor, LineWidth), new Rectangle(X - R + DeltaX, Y - R + DeltaY, R * 2, R * 2));
        }

        public override bool Check(in int X, in int Y, in int R)
        {
            return (Math.Pow(this.X - X, 2) +
            Math.Pow(this.Y - Y, 2) <=
            Math.Pow(R, 2));
        }
    }
}