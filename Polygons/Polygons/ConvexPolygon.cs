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
    public class ConvexPolygon : IEquatable<ConvexPolygon>, IOriginator
    {
        #region Properties
        public Color VertexColor { get; set; }
        public Color LineColor { get; set; }
        public int LineWidth { get; set; }
        public int VertexRadius { get; set; }
        #endregion

        private readonly List<Vertex> _vertices;
        public int Count => _vertices.Count;
        public ReadOnlyCollection<Vertex> Vertices => _vertices.AsReadOnly();

        #region Constructors
        public ConvexPolygon(in Color? VertexColor = null, in Color? LineColor = null, in int? LineWidth = null, in int? VertexR = null)
        {
            _vertices = new List<Vertex>();

            this.VertexColor = VertexColor.GetValueOrDefault(Color.DarkCyan);
            this.LineColor = LineColor.GetValueOrDefault(Color.Cyan);
            this.LineWidth = LineWidth ?? 6;
            this.VertexRadius = VertexR ?? 30;
        }

        public ConvexPolygon(in List<Vertex> Vertices, in Color? VertexColor = null, in Color? LineColor = null, in int LineWidth = 6, in int VertexR = 30)
        {
            _vertices = new List<Vertex>(Vertices ?? throw new ArgumentNullException(nameof(Vertices)));
            ConvexHull(new List<IPolygonCommand>());

            this.VertexColor = VertexColor.GetValueOrDefault(Color.DarkCyan);
            this.LineColor = LineColor.GetValueOrDefault(Color.Cyan);
            this.LineWidth = LineWidth;
            this.VertexRadius = VertexR;
        }

        public ConvexPolygon(in ConvexPolygon other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            _vertices = new List<Vertex>(other._vertices);

            VertexColor = other.VertexColor;
            LineColor = other.LineColor;
            LineWidth = other.LineWidth;
            VertexRadius = other.VertexRadius;
        }
        #endregion

        #region Memento

        public IMemento CreateMemento()
        {
            return new PolygonMemento(VertexColor, LineColor, LineWidth, VertexRadius, _vertices);
        }

        public void SetMemento(in IMemento memento)
        {
            if (memento == null) throw new ArgumentNullException(nameof(memento));
            dynamic state = (memento as PolygonMemento).GetState();

            VertexColor = state.VertexColor;
            LineColor = state.LineColor;
            LineWidth = state.LineWidth;
            VertexRadius = state.VertexRadius;
            _vertices.Clear(); _vertices.AddRange(state.Vertices);
        }

        #endregion

        #region List-like actions
        public IList<IPolygonCommand> Add(in Vertex newItem)
        {
            _vertices.Add(newItem ?? throw new ArgumentNullException(nameof(newItem)));
            return new List<IPolygonCommand>() { new VertexAdd(newItem) };
        }

        public IList<IPolygonCommand> AddRange(in IList<Vertex> newItems)
        {
            if (newItems == null) throw new ArgumentNullException(nameof(newItems));
            if (newItems.Count < 1) throw new ArgumentException();
            _vertices.AddRange(newItems);
            return ConvexHull(new List<IPolygonCommand>(newItems.Select(i => new VertexAdd(i))));
        }

        public IList<IPolygonCommand> RemoveAt(in int index)
        {
            if (index < 0 || index >= Count - 1) throw new IndexOutOfRangeException("index");
            var action = new VertexDelete(_vertices[index]);
            _vertices.RemoveAt(index);
            return new List<IPolygonCommand>() { action };
        }

        public IList<IPolygonCommand> Remove(in Vertex item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            var action = new VertexDelete(item);
            _vertices.Remove(item);
            return new List<IPolygonCommand>() { action };
        }

        public IList<IPolygonCommand> RemoveRange(in int start, int count)
        { // start is the first element to remove. The next count - 1 elements will be removed as well
            if (start < 0 || start + count > Count || count < 0) throw new ArgumentOutOfRangeException();
            var history = new List<IPolygonCommand>();
            if (count == 0) return history;
            for (; count != 0; --count)
            {
                history.Add(new VertexDelete(_vertices[count]));
                _vertices.RemoveAt(count);
            }
            return history;
        }

        public IList<IPolygonCommand> RemoveRange(in IEnumerable<Vertex> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            var history = new List<IPolygonCommand>();
            foreach (var i in items)
            {
                history.Add(new VertexDelete(i));
                _vertices.Remove(i);
            }
            return history;
        }

        public int IndexOf(in Vertex item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            return _vertices.IndexOf(item);
        }
        #endregion

        private static int VectorMul(in Point a1, in Point a2, in Point b1, in Point b2)
        {
            Point v1 = new Point(a2.X - a1.X, a1.Y - a2.Y);
            Point v2 = new Point(b2.X - b1.X, b1.Y - b2.Y);
            return (v1.X) * (v2.Y) - (v2.X) * (v1.Y);
        }

        public bool Check(in Point point)
        { // O(log n)
            if (point == null) throw new ArgumentNullException(nameof(point));
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

            if ((VectorMul(_vertices[L], _vertices[R % _vertices.Count], _vertices[L], point) > 0) ==
                (VectorMul(_vertices[L], _vertices[R % _vertices.Count], _vertices[L], _vertices[OtherVertexIndex]) > 0))
                return true;
            return false;
        }

        public IList<ICommand> ConvexHull() => ConvexHull(new List<ICommand>());

        public IList<ICommand> ConvexHull(in IList<ICommand> history)
        { // O(n log n)
            if (history == null) throw new ArgumentNullException(nameof(history));

            if (_vertices.Count < 3)
                return history;
            // поиск самой нижней точки, если несколько - с наибольшим X; O(n)
            int LowestRightestIndex = 0;
            for (int i = 1; i < _vertices.Count; i++)
            {
                if (_vertices[i].Y > _vertices[LowestRightestIndex].Y ||
                    (_vertices[i].Y == _vertices[LowestRightestIndex].Y && _vertices[i].X > _vertices[LowestRightestIndex].X))
                    LowestRightestIndex = i;
            }
            // C# Preview version (_vertices[^1], _vertices[LowestRightestIndex]) = (_vertices[LowestRightestIndex], _vertices[^1]);
            (_vertices[_vertices.Count - 1], _vertices[LowestRightestIndex]) = (_vertices[LowestRightestIndex], _vertices[_vertices.Count - 1]); // ставим её в конец списка
                                                                                                                                                 // сортировка по полярному углу относительно последней точки Vertices[Vertices.Count - 1]; O(n log n)
            _vertices.Sort((Vertex A, Vertex B) =>
                {
                    if (VectorMul(_vertices[_vertices.Count - 1], A, _vertices[_vertices.Count - 1], B) > 0)
                        return -1;
                    else
                        return 1;
                });
            // правка выпуклости; O(n)
            for (int i = 2; i < _vertices.Count; i++)
            {
                while (i > 1 && _vertices.Count > 1 &&
                       0 >= VectorMul(_vertices[i - 2], _vertices[i - 1],
                                      _vertices[i - 1], _vertices[i]))
                {
                    history.Add(new VertexDelete(_vertices[i - 1]));
                    _vertices.RemoveAt(i - 1);
                    i--;
                }
            }
            return history;
        }

        public void Draw(in Graphics e, in Random rand = null)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (Count < 1) return;

            List<Point> Hull = new List<Point>();
            //Hull.AddRange(from )
            for (int i = 0; i < _vertices.Count; i++)
            {
                Hull.Add(new Point(_vertices[i].X + rand?.Next(-2, 3) ?? 0,
                                   _vertices[i].Y + rand?.Next(-2, 3) ?? 0));
            }

            if (Count > 2)
            {
                if (DragAndDropManager.Dropping)
                    e.DrawPolygon(new Pen(LineColor, LineWidth), new ConvexPolygon(_vertices)._vertices.ConvertAll(i => new Point(i.X, i.Y)).ToArray());
                else
                    e.DrawPolygon(new Pen(LineColor, LineWidth), _vertices.ConvertAll(i => new Point(i.X, i.Y)).ToArray());
            }

            foreach (var i in _vertices)
                i.Draw(e, Count > 2, VertexColor, LineColor, LineWidth, VertexRadius, rand?.Next(-2, 3) ?? 0, rand?.Next(-2, 3) ?? 0);
        }

        public List<Vertex> CheckVertices(in Point pointerLocation)
        {
            List<Vertex> Clicked = new List<Vertex>();
            foreach (var i in _vertices)
                if (i.Check(pointerLocation, VertexRadius))
                    Clicked.Add(i);
            return Clicked;
        }

        #region Overrides
        public override int GetHashCode()
        {
            //int hashcode = field1.GetHashCode();
            //hashcode = 31 * hashcode + field2.GetHashCode();
            //hashcode = 31 * hashcode + field3.GetHashCode();
            // и т.д. для остальный полей
            //return hashcode;
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ConvexPolygon);
        }

        public bool Equals(ConvexPolygon other)
        {
            return other != null &&
                   EqualityComparer<Color>.Default.Equals(VertexColor, other.VertexColor) &&
                   EqualityComparer<Color>.Default.Equals(LineColor, other.LineColor) &&
                   LineWidth == other.LineWidth &&
                   VertexRadius == other.VertexRadius &&
                   EqualityComparer<List<Vertex>>.Default.Equals(_vertices, other._vertices) &&
                   Count == other.Count &&
                   EqualityComparer<ReadOnlyCollection<Vertex>>.Default.Equals(Vertices, other.Vertices);
        }

        public Vertex this[in int index] => _vertices[index];

        public static bool operator ==(ConvexPolygon left, ConvexPolygon right)
        {
            return EqualityComparer<ConvexPolygon>.Default.Equals(left, right);
        }

        public static bool operator !=(ConvexPolygon left, ConvexPolygon right)
        {
            return !(left == right);
        }
        #endregion
    }

    [Serializable]
    public class PolygonMemento : IMemento//Todo:, IEquatable<PolygonMemento>
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
            _vertices = vertices.AsReadOnly();
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
