using Figures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

namespace Polygons
{
    [Serializable]
    public class ConvexPolygon //: IOriginator
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
        public ConvexPolygon(in Color? vertexColor = null, in Color? lineColor = null, in int? lineWidth = null, in int? vertexR = null)
        {
            _vertices = new List<Vertex>();

            this.VertexColor = vertexColor.GetValueOrDefault(Color.DarkCyan);
            this.LineColor = lineColor.GetValueOrDefault(Color.Cyan);
            this.LineWidth = lineWidth ?? 6;
            this.VertexRadius = vertexR ?? 30;
        }

        public ConvexPolygon(in List<Vertex> vertices, in Color? vertexColor = null, in Color? lineColor = null, in int lineWidth = 6, in int vertexR = 30)
        {
            _vertices = new List<Vertex>(vertices ?? throw new ArgumentNullException(nameof(vertices)));
            ConvexHull(new List<IPolygonCommand>());

            this.VertexColor = vertexColor.GetValueOrDefault(Color.DarkCyan);
            this.LineColor = lineColor.GetValueOrDefault(Color.Cyan);
            this.LineWidth = lineWidth;
            this.VertexRadius = vertexR;
        }

        public ConvexPolygon(in ConvexPolygon other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            ConvexHull(new List<IPolygonCommand>());

            VertexColor = other.VertexColor;
            LineColor = other.LineColor;
            LineWidth = other.LineWidth;
            VertexRadius = other.VertexRadius;
        }
        #endregion

        #region List-like actions
        public IList<IPolygonCommand> Add(in Vertex newItem)
        {
            _vertices.Add(newItem ?? throw new ArgumentNullException("NewItem"));
            return new List<IPolygonCommand>() { new VertexAdd(newItem) };
        }

        public IList<IPolygonCommand> AddRange(in IList<Vertex> newItems)
        {
            if (newItems == null) throw new ArgumentNullException("NewItems");
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
            if (items == null) throw new ArgumentNullException("Items");

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
            var v1 = new Point(a2.X - a1.X, a1.Y - a2.Y);
            var v2 = new Point(b2.X - b1.X, b1.Y - b2.Y);
            return (v1.X) * (v2.Y) - (v2.X) * (v1.Y);
        }

        public bool Check(in Point point) // O(log n)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            if (_vertices.Count < 3)
                return false;

            int l = 0, r = _vertices.Count;

            for (var mid = (r + l) / 2; r - l > 1; mid = (r + l) / 2)
            {
                if (VectorMul(_vertices[0], _vertices[mid], _vertices[0], point) > 0)
                    l = mid;
                else
                    r = mid;
            }

            var otherVertexIndex = r + 1;
            if (otherVertexIndex >= _vertices.Count)
                otherVertexIndex = l - 1;

            if ((VectorMul(_vertices[l], _vertices[r % _vertices.Count], _vertices[l], point) > 0) ==
                (VectorMul(_vertices[l], _vertices[r % _vertices.Count], _vertices[l], _vertices[otherVertexIndex]) > 0))
                return true;
            return false;
        }

        public IList<IPolygonCommand> ConvexHull() => ConvexHull(new List<IPolygonCommand>());

        public IList<IPolygonCommand> ConvexHull(in IList<IPolygonCommand> history) // O(n log n)
        {
            if (history == null) throw new ArgumentNullException(nameof(history));

            if (_vertices.Count < 3)
                return history;
            // поиск самой нижней точки, если несколько - с наибольшим X; O(n)
            var lowestRightestIndex = 0;
            for (var i = 1; i < _vertices.Count; i++)
            {
                if (_vertices[i].Y > _vertices[lowestRightestIndex].Y ||
                    (_vertices[i].Y == _vertices[lowestRightestIndex].Y && _vertices[i].X > _vertices[lowestRightestIndex].X))
                    lowestRightestIndex = i;
            }
            (_vertices[_vertices.Count - 1], _vertices[lowestRightestIndex]) = (_vertices[lowestRightestIndex], _vertices[_vertices.Count - 1]); // ставим её в начало списка
                                                                                                                                                 // сортировка по полярному углу относительно первой точки Vertices[0] O(n log n)
            _vertices.Sort(
                delegate (Vertex a, Vertex b)
                {
                    if (VectorMul(_vertices[_vertices.Count - 1], a, _vertices[_vertices.Count - 1], b) > 0)
                        return -1;
                    else
                        return 1;
                });
            // правка выпуклости; O(n)
            for (var i = 2; i < _vertices.Count; i++)
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

            var hull = new List<Point>();
            //Hull.AddRange(from )
            for (var i = 0; i < _vertices.Count; i++)
            {
                hull.Add(new Point(_vertices[i].X + rand?.Next(-2, 3) ?? 0,
                                   _vertices[i].Y + rand?.Next(-2, 3) ?? 0));
            }

            if (Count > 2)
            {

                if (DragAndDropManager.Dropping)
                    e.DrawPolygon(new Pen(LineColor, LineWidth), (new ConvexPolygon(_vertices))._vertices.ConvertAll(i => new Point(i.X, i.Y)).ToArray());
                else
                    e.DrawPolygon(new Pen(LineColor, LineWidth), _vertices.ConvertAll(i => new Point(i.X, i.Y)).ToArray());
            }

            foreach (var i in _vertices)
                i.Draw(e, Count > 2, VertexColor, LineColor, LineWidth, VertexRadius, rand?.Next(-2, 3) ?? 0, rand?.Next(-2, 3) ?? 0);
        }

        public List<Vertex> CheckVertices(in Point pointerLocation)
        {
            var clicked = new List<Vertex>();
            foreach (var i in _vertices)
                if (i.Check(pointerLocation, VertexRadius))
                    clicked.Add(i);
            return clicked;
        }

        //public Point this[in int index] => new Point(_vertices[index].X, _vertices[index].Y);
        public Vertex this[in int index] => _vertices[index];

        //public static implicit operator ReadOnlyCollection<Vertex>(in ConvexPolygon obj) => obj._vertices.AsReadOnly();
    }
}

#region Memento
/*
public IMemento GetMemento()
{
    return new PolygonMemento(_vertexColor, _lineColor, _lineWidth, _vertexR, _vertices, _dnd_Vertices, _dnd_Vertices_Last_Positions, _actions);
}

public void SetMemento(in IMemento memento)
{
    dynamic mem = ((PolygonMemento)memento);
    _vertexColor = mem.VertexColor;
     _lineColor = mem.LineColor;
    _lineWidth = mem.LineWidth;
    _vertexR = mem.VertexR;

    _vertices = new List<Vertex>(mem.Vertices);
    _dnd_Vertices = new List<Vertex>(mem.Dnd_Vertices);
    Dnd_Vertices_Last_Positions = new List<Point>(_dnd_Vertices_Last_Positions);
    Actions = new ActionList(_actions);
}
*/
#endregion

/*
[Serializable]
public class PolygonMemento : IMemento
{
    private readonly Color _vertexColor;
    private readonly Color _lineColor;
    private readonly int _lineWidth;
    private readonly int _vertexR;

    private readonly List<Vertex> _vertices;
    private readonly List<Vertex> _dnd_Vertices;
    private readonly List<Point> _dnd_Vertices_Last_Positions;
    private readonly ActionList _actions;

    public PolygonMemento(in Color VertexColor, in Color LineColor, in int LineWidth, in int VertexR,
                          in List<Vertex> Vertices, in List<Vertex> Dnd_Vertices,
                          in List<Point> Dnd_Vertices_Last_Positions, in ActionList Actions)
    {
        _vertexColor = VertexColor;
        _lineColor = LineColor;
        _lineWidth = LineWidth;
        _vertexR = VertexR;

        _vertices = new List<Vertex>(Vertices);
        _dnd_Vertices = new List<Vertex>(Dnd_Vertices);
        _dnd_Vertices_Last_Positions = new List<Point>(Dnd_Vertices_Last_Positions);
        _actions = new ActionList(Actions);
    }

    public ExpandoObject GetState()
    {
        dynamic state = new ExpandoObject();

        state.VertexColor = _vertexColor;
        state.LineColor = _lineColor;
        state.LineWidth = _lineWidth;
        state.VertexR = _vertexR;

        state.Vertices = new List<Vertex>(_vertices);
        state.Dnd_Vertices = new List<Vertex>(_dnd_Vertices);
        state.Dnd_Vertices_Last_Positions = new List<Point>(_dnd_Vertices_Last_Positions);
        state.Actions = new ActionList(_actions);

        return state;
    }
}
*/