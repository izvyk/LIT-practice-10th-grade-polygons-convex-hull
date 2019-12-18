using System;
using System.Drawing;

namespace Figures
{
    [Serializable]
    public abstract class Vertex
    {
        public int X { get; set; }
        public int Y { get; set; }

        public int Dx { get; set; } // for Vertex dnd
        public int Dy { get; set; } // for Vertex dnd

        protected Vertex(in int x, in int y)
        {
            this.X = x;
            this.Y = y;
        }

        public abstract void Draw(in Graphics e, in bool drawBorder, in Color vertexColor, in Color lineColor, in int lineWidth, in int r, in int deltaX = 0, in int deltaY = 0); // DeltaX/DeltaY - сдвиг рисунка на (DeltaX; DeltaY) относительно настоящих координат

        public abstract bool Check(in int x, in int y, in int r); // принадлежность точки фигуре
        public virtual bool Check(in Point p, in int r) => Check(p.X, p.Y, r); // перегруженный метод для Point

        public static implicit operator Point(in Vertex i) => new Point(i.X, i.Y);
    }
}