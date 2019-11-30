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

        protected Vertex(in int X, in int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public abstract void Draw(in Graphics e, in bool DrawBorder, in Color VertexColor, in Color LineColor, in int LineWidth, in int R, in int DeltaX = 0, in int DeltaY = 0); // DeltaX/DeltaY - сдвиг рисунка на (DeltaX; DeltaY) относительно настоящих координат

        public abstract bool Check(in int X, in int Y, in int R); // принадлежность точки фигуре
        public virtual bool Check(in Point p, in int R) => Check(p.X, p.Y, R); // перегруженный метод для Point

        public static implicit operator Point(in Vertex i) => new Point(i.X, i.Y);
    }
}