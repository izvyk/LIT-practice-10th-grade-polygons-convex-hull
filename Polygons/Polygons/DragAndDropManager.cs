ï»¿using Figures;
using System.Collections.Generic;
using System.Drawing;

namespace Polygons
{
    public static class DragAndDropManager
    {
        private static IReadOnlyCollection<Vertex> _objects;
        private static readonly List<Point> _startPosition;
        public static bool Dropping { get; private set; }
        public static IReadOnlyCollection<Point> StartPositions => _startPosition.AsReadOnly();
        public static IReadOnlyCollection<Vertex> DragObjects => _objects;

        static DragAndDropManager()
        {
            _objects = new List<Vertex>();
            _startPosition = new List<Point>();
        }

        public static void Start(in Point pointerLocation, IReadOnlyCollection<Vertex> vertices)
        {
            _startPosition.Clear();
            _objects = vertices;
            Dropping = true;
            foreach (Vertex i in _objects)
            {
                _startPosition.Add(i);

                i.Dx = i.X - pointerLocation.X;
                i.Dy = i.Y - pointerLocation.Y;
                i.X = pointerLocation.X + i.Dx;
                i.Y = pointerLocation.Y + i.Dy;
            }
        }

        public static void Stop()
        {
            if (!Dropping) return;
            Dropping = false;

            foreach (Vertex item in _objects)
                item.Dx = item.Dy = 0;
        }

        public static void Tick(in Point pointerPosition)
        {
            if (!Dropping) return;
            foreach (Vertex item in _objects)
            {
                item.X = pointerPosition.X + item.Dx;
                item.Y = pointerPosition.Y + item.Dy;
            }
        }
    }

}
