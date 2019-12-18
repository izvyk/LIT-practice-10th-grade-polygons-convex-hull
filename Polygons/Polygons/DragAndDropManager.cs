using Figures;
using System.Collections.Generic;
using System.Drawing;

namespace Polygons
{
    public static class DragAndDropManager
    {
        private static readonly List<Point> StartPosition;
        public static bool Dropping { get; private set; }
        public static IReadOnlyCollection<Point> StartPositions => StartPosition.AsReadOnly();
        public static IReadOnlyCollection<Vertex> DragObjects { get; private set; }

        static DragAndDropManager()
        {
            DragObjects = new List<Vertex>();
            StartPosition = new List<Point>();
        }

        public static void Start(in Point pointerLocation, IReadOnlyCollection<Vertex> vertices)
        {
            StartPosition.Clear();
            DragObjects = vertices;
            Dropping = true;
            foreach (var i in DragObjects)
            {
                StartPosition.Add(i);

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

            foreach (var item in DragObjects)
                item.Dx = item.Dy = 0;
            StartPosition.Clear();
        }

        public static void Tick(in Point pointerPosition)
        {
            if (!Dropping) return;

            foreach (var item in DragObjects)
            {
                item.X = pointerPosition.X + item.Dx;
                item.Y = pointerPosition.Y + item.Dy;
            }
        }
    }

}
