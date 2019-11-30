using System;
using System.Drawing;

namespace Figures
{
    [Serializable]
    public class Square : Vertex
    {
        public Square(in int X, in int Y) : base(X, Y) { }

        public override void Draw(in Graphics e, in bool DrawBorder, in Color VertexColor, in Color LineColor, in int LineWidth, in int R, in int DeltaX = 0, in int DeltaY = 0)
        {
            e.FillRectangle(new SolidBrush(VertexColor), X - R + DeltaX, Y - R + DeltaY, R * 2, R * 2);
            if (DrawBorder) e.DrawRectangle(new Pen(LineColor, LineWidth), X - R + DeltaX, Y - R + DeltaY, R * 2, R * 2);
        }

        public override bool Check(in int X, in int Y, in int R)
        {
            return (new Rectangle(this.X - R, this.Y - R, R * 2, R * 2)).Contains(X, Y);
        }
    }
}