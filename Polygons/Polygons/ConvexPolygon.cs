using Figures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Dynamic;
using System.Linq;

namespace Polygons
{
    // Todo: equals for all non-static classes
    // Todo: make class diagram
    // Todo: написать документацию
    public class ConvexPolygon : IOriginator
    {
        private List<Vertex> _vertices;
        #region Properties
        public Color VertexColor { get; set; }
        public Color LineColor { get; set; }
        public int LineWidth { get; set; }
        public int VertexRadius { get; set; }
        public int Count => _vertices.Count;
        public ReadOnlyCollection<Vertex> Vertices => _vertices.AsReadOnly();
        public ReadOnlyCollection<Vertex> GetConvexHull => ConvexHull(_vertices).AsReadOnly();
        #endregion


        #region Constructors
        public ConvexPolygon()
        {
            _vertices = new List<Vertex>();

            VertexColor = Color.DarkCyan;
            LineColor = Color.Cyan;
            LineWidth = 6;
            VertexRadius = 30;
        }

        public ConvexPolygon(in List<Vertex> vertices, in Color? vertexColor = null, in Color? lineColor = null, in int lineWidth = 6, in int vertexR = 30)
        {
            _vertices = new List<Vertex>(vertices ?? throw new ArgumentNullException(nameof(vertices)));
            MakeConvex();

            VertexColor = vertexColor.GetValueOrDefault(Color.DarkCyan);
            LineColor = lineColor.GetValueOrDefault(Color.Cyan);
            LineWidth = lineWidth;
            VertexRadius = vertexR;
        }

        #endregion

        #region Memento

        public IMemento CreateMemento()
        {
            return new PolygonMemento(VertexColor, LineColor, LineWidth, VertexRadius, _vertices);
        }

        public void SetMemento(in IMemento memento)
        {
            if (memento is null) throw new ArgumentNullException(nameof(memento));
            dynamic state = ((PolygonMemento) memento).GetState();

            VertexColor = state.VertexColor;
            LineColor = state.LineColor;
            LineWidth = state.LineWidth;
            VertexRadius = state.VertexRadius;
            _vertices.Clear(); _vertices.AddRange(state.Vertices);
        }

        #endregion

        #region List-like actions
        public void Add(in Vertex newItem)
        {
            _vertices.Add(newItem ?? throw new ArgumentNullException(nameof(newItem)));
            MakeConvex();
        }

        public bool Remove(in Vertex item)
        {
            return _vertices.Remove(item);
        }
        
        #endregion

        private static int VectorMul(in Point a1, in Point a2, in Point b1, in Point b2)
        {
            var v1 = new Point(a2.X - a1.X, a1.Y - a2.Y);
            var v2 = new Point(b2.X - b1.X, b1.Y - b2.Y);
            return v1.X * v2.Y - v2.X * v1.Y;
        }

        public bool Contains(in Point point)
        { // O(log n)
            if (_vertices.Count < 3)
                return false;

            int L = 0, R = _vertices.Count;

            for (int mid = (R + L) / 2; R - L > 1; mid = (R + L) / 2)
            {
                if (VectorMul(_vertices[0], _vertices[mid], _vertices[0], point) > 0)
                    L = mid;
                else
                    R = mid;
            }

            int OtherVertexIndex = R + 1;
            if (OtherVertexIndex >= _vertices.Count)
                OtherVertexIndex = L - 1;

            if (VectorMul(_vertices[L], _vertices[R % _vertices.Count], _vertices[L], point) > 0 ==
                VectorMul(_vertices[L], _vertices[R % _vertices.Count], _vertices[L], _vertices[OtherVertexIndex]) > 0)
                return true;
            return false;
        }

        private static List<Vertex> ConvexHull(in List<Vertex> input)
        {
            var hull = new List<Vertex>(input);
            if (hull.Count < 3)
                return hull;

            // поиск самой нижней точки, если несколько - с наибольшим X; O(n)
            var LowestRightestIndex = 0;
            for (var i = 1; i < hull.Count; i++)
            {
                if (hull[i].Y > hull[LowestRightestIndex].Y ||
                    hull[i].Y == hull[LowestRightestIndex].Y && hull[i].X > hull[LowestRightestIndex].X)
                    LowestRightestIndex = i;
            }
            // C# Preview version (hull[^1], hull[LowestRightestIndex]) = (hull[LowestRightestIndex], hull[^1]);
            (hull[hull.Count - 1], hull[LowestRightestIndex]) = (hull[LowestRightestIndex], hull[hull.Count - 1]); // ставим её в конец списка
                                                                                                                                                 // сортировка по полярному углу относительно последней точки Vertices[Vertices.Count - 1]; O(n log n)
            hull.Sort((left, right) =>
                {
                    if (VectorMul(hull[hull.Count - 1], left, hull[hull.Count - 1], right) > 0)
                        return -1;
                    return 1;
                });

            // правка выпуклости; O(n)
            for (var i = 2; i < hull.Count; ++i)
            {
                while (i > 1 && hull.Count > 1 &&
                       0 >= VectorMul(hull[i - 2], hull[i - 1],
                                      hull[i - 1], hull[i]))
                {
                    hull.RemoveAt(i - 1);
                    i--;
                }
            }

            return hull;
        }

        public void MakeConvex()
        {
            _vertices = ConvexHull(_vertices);
        }

        public void Draw(in Graphics e)
        {
            if (e is null) throw new ArgumentNullException(nameof(e));
            if (Count == 0) return;

            if (Count > 2)
                e.DrawPolygon(new Pen(LineColor, LineWidth), ConvexHull(_vertices).ConvertAll(t => new Point(t.X, t.Y)).ToArray());

            foreach (var i in _vertices)
                i.Draw(e, Count > 2, VertexColor, LineColor, LineWidth, VertexRadius);
        }

        public IEnumerable<Vertex> CheckVertices(Point pointerLocation)
        {
            return _vertices.Where(i => i.Check(pointerLocation, VertexRadius));
        }

    }

    [Serializable]
    public class PolygonMemento : IMemento
    {
        private readonly Color _vertexColor;
        private readonly Color _lineColor;
        private readonly int _lineWidth;
        private readonly int _vertexRadius;
        private readonly ReadOnlyCollection<Vertex> _vertices;

        public PolygonMemento(in Color vertexColor, in Color lineColor, in int lineWidth, in int vertexRadius, in List<Vertex> vertices)
        {
            _vertexColor = vertexColor;
            _lineColor = lineColor;
            _lineWidth = lineWidth;
            _vertexRadius = vertexRadius;
            _vertices = vertices?.AsReadOnly() ?? throw new ArgumentNullException(nameof(vertices));
        }

        public ExpandoObject GetState()
        {
            dynamic state = new ExpandoObject();

            state.VertexColor = _vertexColor;
            state.LineColor = _lineColor;
            state.LineWidth = _lineWidth;
            state.VertexRadius = _vertexRadius;
            state.Vertices = new List<Vertex>(_vertices);

            return state;
        }

    }

}
