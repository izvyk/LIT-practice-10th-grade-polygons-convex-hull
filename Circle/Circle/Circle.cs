using System;
using System.Drawing;

namespace Figures
{
    [Serializable]
    public class Circle : Vertex
    {
        public Circle(in int x, in int y) : base(x, y) { }

        public override void Draw(in Graphics e, in bool drawBorder, in Color vertexColor, in Color lineColor, in int lineWidth, in int r, in int deltaX = 0, in int deltaY = 0)
        {
            e.FillEllipse(new SolidBrush(vertexColor), new Rectangle(X - r + deltaX, Y - r + deltaY, r * 2, r * 2));
            if (drawBorder) e.DrawEllipse(new Pen(lineColor, lineWidth), new Rectangle(X - r + deltaX, Y - r + deltaY, r * 2, r * 2));
        }

        public override bool Check(in int x, in int y, in int r)
        {
            return
                Math.Pow(X - x, 2) +
                Math.Pow(Y - y, 2) <=
                Math.Pow(r, 2);
        }
    }
}