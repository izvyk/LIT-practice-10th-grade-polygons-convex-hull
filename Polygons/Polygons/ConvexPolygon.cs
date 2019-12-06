using Figures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Polygons
{
    [Serializable]
    public class ConvexPolygon //: IOriginator
    {
        #region History
        public interface IActionPolygon
        {
            void Undo(in ConvexPolygon Polygon);

            void Redo(in ConvexPolygon Polygon);
        }

        [Serializable]
        public class AddVertex : IActionPolygon
        {
            private readonly Vertex Item;

            public AddVertex(in Vertex Item) => this.Item = Item;

            public void Undo(in ConvexPolygon Polygon)
            {
                if (Polygon == null) throw new ArgumentNullException();
                Polygon._vertices.Remove(Item);
            }

            public void Redo(in ConvexPolygon Polygon)
            {
                if (Polygon == null) throw new ArgumentNullException();
                Polygon._vertices.Add(Item);
            }
        }

        [Serializable]
        public class DeleteVertex : IActionPolygon
        {
            private readonly Vertex Item;

            public DeleteVertex(in Vertex Item) => this.Item = Item;

            public void Undo(in ConvexPolygon Polygon)
            {
                if (Polygon == null) throw new ArgumentNullException();
                //a?.Add(Item) ?? throw new ArgumentNullException();
                Polygon._vertices.Add(Item);
            }

            public void Redo(in ConvexPolygon Polygon)
            {
                if (Polygon == null) throw new ArgumentNullException();
                Polygon._vertices.Remove(Item);
            }
        }

        [Serializable]
        public class VertexColorChanged : IActionPolygon
        {
            private readonly Color NewColor, PrevColor;

            public VertexColorChanged(in Color PrevColor, in Color NewColor)
            {
                this.PrevColor = PrevColor;
                this.NewColor = NewColor;
            }
            public void Undo(in ConvexPolygon Polygon)
            {
                if (Polygon == null) throw new ArgumentNullException("form");
                Polygon._vertexColor = PrevColor;
            }

            public void Redo(in ConvexPolygon Polygon)
            {
                if (Polygon == null) throw new ArgumentNullException("form");
                Polygon._vertexColor = NewColor;
            }
        }

        [Serializable]
        public class LineColorChanged : IActionPolygon
        {
            private readonly Color NewColor, PrevColor;

            public LineColorChanged(in Color PrevColor, in Color NewColor)
            {
                this.PrevColor = PrevColor;
                this.NewColor = NewColor;
            }
            public void Undo(in ConvexPolygon Polygon)
            {
                if (Polygon == null) throw new ArgumentNullException("form");
                Polygon._lineColor = PrevColor;
            }

            public void Redo(in ConvexPolygon Polygon)
            {
                if (Polygon == null) throw new ArgumentNullException("form");
                Polygon._lineColor = NewColor;
            }
        }

        [Serializable]
        public class LineWidthChanged : IActionPolygon
        {
            private readonly int NewWidth, PrevWidth;

            public LineWidthChanged(in int PrevWidth, in int NewWidth)
            {
                this.PrevWidth = PrevWidth;
                this.NewWidth = NewWidth;
            }
            public void Undo(in ConvexPolygon Polygon) => Polygon._lineWidth = PrevWidth;

            public void Redo(in ConvexPolygon Polygon) => Polygon._lineWidth = NewWidth;
        }

        [Serializable]
        public class VertexRadiusChanged : IActionPolygon
        {
            private readonly int NewRadius, PrevRadius;

            public VertexRadiusChanged(in int NewRadius, in int PrevRadius)
            {
                this.PrevRadius = NewRadius;
                this.NewRadius = PrevRadius;
            }
            public void Undo(in ConvexPolygon Polygon) => Polygon._vertexR = PrevRadius;

            public void Redo(in ConvexPolygon Polygon) => Polygon._vertexR = NewRadius;
        }

        [Serializable]
        public class VertexPositionChanged : IActionPolygon
        {
            private readonly Point NewPosition, PrevPosition;
            private readonly Vertex LinkToObject;

            public VertexPositionChanged(in Vertex Item, in Point NewPosition)
            {
                this.NewPosition = NewPosition;
                PrevPosition = new Point(Item.X, Item.Y);
                LinkToObject = Item;
            }
            public VertexPositionChanged(in Point PreviousPosition, in Vertex Item)
            {
                PrevPosition = PreviousPosition;
                NewPosition = new Point(Item.X, Item.Y);
                LinkToObject = Item;
            }
            public void Undo(in ConvexPolygon Polygon)
            {
                int index = Polygon._vertices.IndexOf(LinkToObject);
                if (index < 0) return;
                Polygon._vertices[index].X = PrevPosition.X;
                Polygon._vertices[index].Y = PrevPosition.Y;
            }

            public void Redo(in ConvexPolygon Polygon)
            {
                int index = Polygon._vertices.IndexOf(LinkToObject);
                if (index < 0) return;
                Polygon._vertices[index].X = NewPosition.X;
                Polygon._vertices[index].Y = NewPosition.Y;
            }
        }
        #endregion

        private Color _vertexColor = ColorTranslator.FromHtml("#073642"); // green
        public Color VertexColor
        {
            get => _vertexColor;
            set
            {
                if (_vertexColor == value) return;
                _actions.Add(new List<IActionPolygon>() { new VertexColorChanged(_vertexColor, value) });
                _vertexColor = value;
                ColorChanged?.Invoke(typeof(ConvexPolygon), EventArgs.Empty);
            }
        }
        private Color _lineColor = ColorTranslator.FromHtml("#657b83"); // base02
        public Color LineColor
        {
            get => _lineColor;
            set
            {
                if (_lineColor == value) return;
                _actions.Add(new List<IActionPolygon>() { new LineColorChanged(_lineColor, value) });
                _lineColor = value;
                ColorChanged?.Invoke(typeof(ConvexPolygon), EventArgs.Empty);
            }
        }
        private int _lineWidth = 6;
        public int LineWidth
        {
            get => _lineWidth;
            set
            {
                if (_lineWidth == value) return;
                _actions.Add(new List<IActionPolygon>() { new LineWidthChanged(_lineWidth, value) });
                _lineWidth = value;
                WidthChanged?.Invoke(typeof(ConvexPolygon), EventArgs.Empty);
            }
        }
        private int _vertexR = 30;
        public int VertexR
        {
            get => _vertexR;
            set
            {
                if (_vertexR == value) return;
                _actions.Add(new List<IActionPolygon>() { new VertexRadiusChanged(_vertexR, value) });
                _vertexR = value;
                WidthChanged?.Invoke(typeof(ConvexPolygon), EventArgs.Empty);
            }
        }
        #region Events
        public delegate void ColorEventHandler(object sender, EventArgs e);
        public static event EventHandler ColorChanged;
        public delegate void WidthEventHandler(object sender, EventArgs e);
        public static event EventHandler WidthChanged;
        #endregion

        private readonly List<Vertex> _vertices;
        private readonly List<Vertex> _dnd_Vertices;
        private readonly List<Point> _dnd_Vertices_Last_Positions;
        private readonly ActionList _actions;
        public bool DragAndDrop => _dnd_Vertices.Count > 0; // Todo: сделать переключателем
        public int Count => _vertices.Count;

        #region Constructors
        public ConvexPolygon(in Color? VertexColor = null, in Color? LineColor = null, in int? LineWidth = null, in int? VertexR = null)
        {
            _vertices = new List<Vertex>();
            _actions = new ActionList();
            _dnd_Vertices = new List<Vertex>();
            _dnd_Vertices_Last_Positions = new List<Point>();

            _vertexColor = VertexColor ?? _vertexColor;
            _lineColor = LineColor ?? _lineColor;
            _lineWidth = LineWidth ?? _lineWidth;
            _vertexR = VertexR ?? _vertexR;
        }

        public ConvexPolygon(in List<Vertex> Vertices, in List<Vertex> Dnd_Vertices = null, in List<Point> Dnd_Vertices_Last_Positions = null, ActionList Actions = null, in Color? VertexColor = null, in Color? LineColor = null, in int LineWidth = 6, in int VertexR = 30)
        {
            this._vertices = new List<Vertex>(Vertices ?? throw new ArgumentNullException("Vertices"));
            this._dnd_Vertices = Dnd_Vertices ?? new List<Vertex>();
            this._dnd_Vertices_Last_Positions = Dnd_Vertices_Last_Positions ?? new List<Point>();
            this._actions = Actions == null ? new ActionList() : new ActionList(Actions);
            ConvexHull(new List<IActionPolygon>());

            _vertexColor = VertexColor.GetValueOrDefault(Color.DarkCyan);
            _lineColor = LineColor.GetValueOrDefault(Color.Cyan);
            _lineWidth = LineWidth;
            _vertexR = VertexR;
        }

        public ConvexPolygon(in ConvexPolygon previous)
        {
            if (previous == null) throw new ArgumentNullException("previous");
            _dnd_Vertices = previous._dnd_Vertices;
            _dnd_Vertices_Last_Positions = previous._dnd_Vertices_Last_Positions;
            _vertices = new List<Vertex>(previous._vertices);
            _actions = new ActionList(previous._actions);
            ConvexHull(new List<IActionPolygon>());

            _vertexColor = previous._vertexColor;
            _lineColor = previous._lineColor;
            _lineWidth = previous._lineWidth;
            _vertexR = previous._vertexR;
        }
        #endregion

        #region List-like actions
        public void Add(in Vertex NewItem)
        {
            _vertices.Add(NewItem ?? throw new ArgumentNullException("NewItem"));
            _actions.Add(ConvexHull(new List<IActionPolygon>() { new AddVertex(NewItem) })); // запоминаем цепочку событий
        }

        public void AddRange(in ICollection<Vertex> NewItems)
        {
            if (NewItems == null) throw new ArgumentNullException("NewItems");
            if (NewItems.Count < 1) return;
            List<IActionPolygon> tmp = new List<IActionPolygon>();
            foreach (var i in NewItems)
            {
                _vertices.Add(i);
                tmp.Add(new AddVertex(i));
            }
            _actions.Add(ConvexHull(tmp)); // запоминаем цепочку событий
        }

        public void RemoveAt(in int index)
        {
            if (index < 0 || index >= Count - 1) throw new IndexOutOfRangeException("index");
            _actions.Add(new List<IActionPolygon> { new DeleteVertex(_vertices[index]) });
            _vertices.RemoveAt(index);
        }

        public void Remove(in Vertex Item)
        {
            if (Item == null) throw new ArgumentNullException("Item");
            _actions.Add(new List<IActionPolygon> { new DeleteVertex(Item) });
            _vertices.Remove(Item);
        }

        public void RemoveRange(in int start, int count)
        { // start is the first element to remove. The next count - 1 elements will be removed as well
            if (start < 0 || start + count > Count || count < 0) throw new ArgumentOutOfRangeException();
            if (count == 0) return;
            var tmp = new List<IActionPolygon>();
            for (; count != 0; --count)
            {
                tmp.Add(new DeleteVertex(_vertices[count]));
                _vertices.RemoveAt(count);
            }
            _actions.Add(tmp);
        }

        public void RemoveRange(in IEnumerable<Vertex> Items)
        {
            if (Items == null) throw new ArgumentNullException("Items");

            var tmp = new List<IActionPolygon>();
            foreach (var i in Items)
            {
                tmp.Add(new DeleteVertex(i));
                _vertices.Remove(i);
            }
            _actions.Add(tmp);
        }
        #endregion

        private static int VectorMul(in Point A1, in Point A2, in Point B1, in Point B2)
        {
            Point v1 = new Point(A2.X - A1.X, A1.Y - A2.Y);
            Point v2 = new Point(B2.X - B1.X, B1.Y - B2.Y);
            return (v1.X) * (v2.Y) - (v2.X) * (v1.Y);
        }

        public bool Check(in Point point) // O(log n)
        {
            if (point == null) throw new ArgumentNullException("point");
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
        private List<IActionPolygon> ConvexHull(in List<IActionPolygon> tmp) // O(n log n)
        {
            if (tmp == null) throw new ArgumentNullException("tmp");

            if (_vertices.Count < 3)
                return tmp;
            // поиск самой нижней точки, если несколько - с наибольшим X; O(n)
            int LowestRightestIndex = 0;
            for (int i = 1; i < _vertices.Count; i++)
            {
                if (_vertices[i].Y > _vertices[LowestRightestIndex].Y ||
                    (_vertices[i].Y == _vertices[LowestRightestIndex].Y && _vertices[i].X > _vertices[LowestRightestIndex].X))
                    LowestRightestIndex = i;
            }
            (_vertices[_vertices.Count - 1], _vertices[LowestRightestIndex]) = (_vertices[LowestRightestIndex], _vertices[_vertices.Count - 1]); // ставим её в начало списка
                                                                                                                                                 // сортировка по полярному углу относительно первой точки Vertices[0] O(n log n)
            _vertices.Sort(
                delegate (Vertex A, Vertex B)
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
                    tmp.Add(new DeleteVertex(_vertices[i - 1]));
                    _vertices.RemoveAt(i - 1);
                    i--;
                }
            }
            return tmp;
        }

        #region DragAndDrop
        public void DragAndDropCheck(in Point PointerLocation)
        {
            if (PointerLocation == null) throw new ArgumentNullException("PointerLocation");

            if (_dnd_Vertices.Count == 0)
            {
                var tmp = CheckVertices(PointerLocation);
                if (tmp.Count > 0)
                    _dnd_Vertices.AddRange(tmp);
                else if (Check(PointerLocation))
                    _dnd_Vertices.AddRange(_vertices);
                foreach (var i in _dnd_Vertices)
                {
                    _dnd_Vertices_Last_Positions.Add(i);

                    i.Dx = i.X - PointerLocation.X;
                    i.Dy = i.Y - PointerLocation.Y;
                    i.X = PointerLocation.X + i.Dx;
                    i.Y = PointerLocation.Y + i.Dy;
                }
            }
            else
            {
                foreach (var i in _dnd_Vertices)
                {
                    i.X = PointerLocation.X + i.Dx;
                    i.Y = PointerLocation.Y + i.Dy;
                }
            }

        }

        public void DragAndDropStop()
        {
            if (!DragAndDrop) return;
            var tmp = new List<IActionPolygon>();
            for (int i = 0; i < _dnd_Vertices.Count; ++i)
            {
                _dnd_Vertices[i].Dx = _dnd_Vertices[i].Dy = 0;
                tmp.Add(new VertexPositionChanged(_dnd_Vertices_Last_Positions[i], _dnd_Vertices[i]));
            }

            _actions.Add(ConvexHull(tmp));
            _dnd_Vertices.Clear();
            _dnd_Vertices_Last_Positions.Clear();
        }
        #endregion

        #region Undo/Redo
        public void Undo()
        {
            if (_actions.UndoCount > 0)
            {
                _actions.Undo(this);
                ConvexHull(new List<IActionPolygon>());
            }
        }

        public void Redo()
        {
            if (_actions.RedoCount > 0)
            {
                _actions.Redo(this);
                ConvexHull(new List<IActionPolygon>());
            }
        }
        #endregion

        public void Draw(in Graphics e, in int? R = null, in int? L = null, in Random rand = null)
        {
            if (e == null) throw new ArgumentNullException("e");
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

                if (DragAndDrop)
                    e.DrawPolygon(new Pen(LineColor, L ?? LineWidth), (new ConvexPolygon(_vertices))._vertices.ConvertAll(i => new Point(i.X, i.Y)).ToArray());
                else
                    e.DrawPolygon(new Pen(LineColor, L ?? LineWidth), _vertices.ConvertAll(i => new Point(i.X, i.Y)).ToArray());
            }

            foreach (var i in _vertices)
                i.Draw(e, Count > 2, VertexColor, LineColor, L ?? LineWidth, R ?? VertexR, rand?.Next(-2, 3) ?? 0, rand?.Next(-2, 3) ?? 0);
        }

        public List<Vertex> CheckVertices(in Point PointerLocation)
        {
            List<Vertex> Clicked = new List<Vertex>();
            foreach (var i in _vertices)
                if (i.Check(PointerLocation, VertexR))
                    Clicked.Add(i);
            return Clicked;
        }

        public Point this[in int index] => new Point(_vertices[index].X, _vertices[index].Y);

        public static implicit operator ReadOnlyCollection<Vertex>(in ConvexPolygon obj) => obj._vertices.AsReadOnly();
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