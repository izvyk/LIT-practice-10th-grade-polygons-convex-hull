using System;
using System.Drawing;

namespace Figures
{
    [Serializable]
    public class Square : Vertex
    {
        public Square(in int x, in int y) : base(x, y) { }

        public override void Draw(in Graphics e, in bool drawBorder, in Color vertexColor, in Color lineColor, in int lineWidth, in int r, in int deltaX = 0, in int deltaY = 0)
        {
            e.FillRectangle(new SolidBrush(vertexColor), X - r + deltaX, Y - r + deltaY, r * 2, r * 2);
            if (drawBorder) e.DrawRectangle(new Pen(lineColor, lineWidth), X - r + deltaX, Y - r + deltaY, r * 2, r * 2);
        }

        public override bool Check(in int x, in int y, in int r)
        {
            return (new Rectangle(X - r, Y - r, r * 2, r * 2)).Contains(x, y);
        }
    }
}