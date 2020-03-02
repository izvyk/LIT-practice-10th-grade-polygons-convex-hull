/*
using System;
using System.Drawing;

namespace Polygons
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

        public abstract bool Contains(in int X, in int Y, in int R); // принадлежность точки фигуре
        public virtual bool Contains(in Point p, in int R) => Contains(p.X, p.Y, R); // перегруженный метод для Point

        public static implicit operator Point(in Vertex i) => new Point(i.X, i.Y);
    }

    [Serializable]
    class Circle : Vertex
    {
        public Circle(in int X, in int Y) : base(X, Y) { }

        public override void Draw(in Graphics e, in bool DrawBorder, in Color VertexColor, in Color LineColor, in int LineWidth, in int R, in int DeltaX = 0, in int DeltaY = 0)
        {
            e.FillEllipse(new SolidBrush(VertexColor), new Rectangle(X - R + DeltaX, Y - R + DeltaY, R * 2, R * 2));
            if (DrawBorder) e.DrawEllipse(new Pen(LineColor, LineWidth), new Rectangle(X - R + DeltaX, Y - R + DeltaY, R * 2, R * 2));
        }

        public override bool Contains(in int X, in int Y, in int R)
        {
            return (Math.Pow(this.X - X, 2) +
            Math.Pow(this.Y - Y, 2) <=
            Math.Pow(R, 2));
        }
    }
}
*/