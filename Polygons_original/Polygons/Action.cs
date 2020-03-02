using Figures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Polygons
{
    /*
    #region Old
    [Serializable]
    public class ActionList
    {
        private readonly Stack<List<ConvexPolygon.IActionPolygon>> ForUndo, ForRedo;
        public int UndoCount => ForUndo.Count;
        public int RedoCount => ForRedo.Count;

        public ActionList()
        {
            ForUndo = new Stack<List<ConvexPolygon.IActionPolygon>>();
            ForRedo = new Stack<List<ConvexPolygon.IActionPolygon>>();
        }

        public ActionList(in ActionList other)
        {
            if (other == null) throw new ArgumentNullException("other");
            ForUndo = new Stack<List<ConvexPolygon.IActionPolygon>>(other.ForUndo);
            ForRedo = new Stack<List<ConvexPolygon.IActionPolygon>>(other.ForRedo);
        }

        public void Add(in List<ConvexPolygon.IActionPolygon> item)
        {
            if (item == null) throw new ArgumentNullException("Item");
            ForUndo.Push(item);
            if (RedoCount > 0 && ForRedo.Peek() != item) ForRedo.Clear();
        }

        public void Undo(in ConvexPolygon polygon)
        {
            if (polygon == null) throw new ArgumentNullException("polygon");
            if (UndoCount < 1) throw new InvalidOperationException("Operation is not valid as there is nothing to undo");

            foreach (ConvexPolygon.IActionPolygon i in ForUndo.Peek())
                i.Undo(polygon);

            ForRedo.Push(ForUndo.Pop());
        }

        public void Redo(in ConvexPolygon polygon)
        {
            if (polygon == null) throw new ArgumentNullException("polygon");
            if (RedoCount < 1) throw new InvalidOperationException("Operation is not valid as there is nothing to redo");
            foreach (var i in ForRedo.Peek())
                i.Redo(polygon);

            ForUndo.Push(ForRedo.Pop());
        }
    }
    #endregion
    */
    public interface ICommand
    {
        void Do(in object obj);
        void Undo(in object obj);
    }

    public interface IPolygonCommand : ICommand { }

    [Serializable]
    public static class HistoryManager
    {
        private static readonly Stack<IEnumerable<ICommand>> ForUndo, ForRedo;
        public static int UndoCount => ForUndo.Count;
        public static int RedoCount => ForRedo.Count;

        static HistoryManager()
        {
            ForUndo = new Stack<IEnumerable<ICommand>>();
            ForRedo = new Stack<IEnumerable<ICommand>>();
        }

        public static void Add(params ICommand[] item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item.Length == 0) throw new ArgumentException();

            ForUndo.Push(item);
            if (RedoCount > 0 && ForRedo.Peek() != item) ForRedo.Clear();
        }

        public static void Undo(in Form1 form1, in ConvexPolygon polygon)
        {
            if (form1 == null) throw new ArgumentNullException(nameof(form1));
            if (polygon == null) throw new ArgumentNullException(nameof(polygon));
            if (UndoCount < 1) throw new InvalidOperationException("Operation is not valid as there is nothing to undo.");

            foreach (var command in (ForUndo.Peek()).Reverse())
            {
                if (command is IPolygonCommand)
                    command.Undo(polygon);
                else
                    command.Undo(form1);
            }

            //for (var i = _forUndo.Peek().Pop(); _forUndo.Peek().Count > 0; i = _forUndo.Peek().Pop())

            ForRedo.Push(ForUndo.Pop());
        }

        public static void Redo(in Form1 form1, in ConvexPolygon polygon)
        {
            if (form1 == null) throw new ArgumentNullException(nameof(form1));
            if (polygon == null) throw new ArgumentNullException(nameof(polygon));
            if (RedoCount < 1) throw new InvalidOperationException("Operation is not valid as there is nothing to redo.");

            foreach (var command in ForRedo.Peek())
            {
                if (command is IPolygonCommand)
                    command.Do(polygon);
                else
                    command.Do(form1);
            }

            ForUndo.Push(ForRedo.Pop());
        }
    }

    #region Commands
    [Serializable] // OK
    public class VertexAdd : IPolygonCommand
    {
        private readonly Vertex _item;

        public VertexAdd(in Vertex item) => _item = item ?? throw new ArgumentNullException("Item");

        public void Undo(in object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            ((ConvexPolygon)obj).Remove(_item);
        }

        public void Do(in object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            ((ConvexPolygon)obj).Add(_item);
        }
    }

    [Serializable] // OK
    public class VertexDelete : IPolygonCommand
    {
        private readonly Vertex _item;

        public VertexDelete(in Vertex item) => _item = item ?? throw new ArgumentNullException("Item");

        public void Do(in object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            ((ConvexPolygon)obj).Remove(_item);
        }

        public void Undo(in object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            ((ConvexPolygon)obj).Add(_item);
        }
    }

    [Serializable] // OK
    public class VertexColorChanged : IPolygonCommand
    {
        private readonly Color _newColor, _prevColor;

        public VertexColorChanged(in Color prevColor, in Color newColor)
        {
            _prevColor = prevColor;
            _newColor = newColor;
        }

        public void Do(in object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            ((ConvexPolygon)obj).VertexColor = _newColor;
        }

        public void Undo(in object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            ((ConvexPolygon)obj).VertexColor = _prevColor;
        }
    }

    [Serializable] // OK
    public class VertexRadiusChanged : IPolygonCommand
    {
        private readonly int _prevRadius, _newRadius;

        public VertexRadiusChanged(in int prevRadius, in int newRadius)
        {
            _prevRadius = prevRadius;
            _newRadius = newRadius;
        }

        public void Do(in object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            ((ConvexPolygon)obj).VertexRadius = _newRadius;
        }

        public void Undo(in object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            ((ConvexPolygon)obj).VertexRadius = _prevRadius;
        }

    }

    [Serializable] // ok
    public class VertexPositionChanged : IPolygonCommand
    {
        private readonly Point _prevPosition, _newPosition;
        private readonly Vertex _linkToObject;

        public VertexPositionChanged(in Vertex item, in Point newPosition)
        {
            _linkToObject = item ?? throw new ArgumentNullException(nameof(item));
            _newPosition = newPosition;
            _prevPosition = new Point(item.X, item.Y);
        }

        public VertexPositionChanged(in Point previousPosition, in Vertex item)
        {
            _linkToObject = item ?? throw new ArgumentNullException(nameof(item));
            _prevPosition = previousPosition;
            _newPosition = new Point(item.X, item.Y);
        }

        public void Do(in object obj)
        {
            var index = ((ConvexPolygon)obj).IndexOf(_linkToObject);
            if (index < 0) throw new InvalidOperationException("Operation is not valid as the needed object is not found.");
            ((ConvexPolygon)obj)[index].X = _newPosition.X;
            ((ConvexPolygon)obj)[index].Y = _newPosition.Y;
        }

        public void Undo(in object obj)
        {
            var index = ((ConvexPolygon)obj).IndexOf(_linkToObject);
            if (index < 0) throw new InvalidOperationException("Operation is not valid as the needed object is not found.");
            ((ConvexPolygon)obj)[index].X = _prevPosition.X;
            ((ConvexPolygon)obj)[index].Y = _prevPosition.Y;
        }
    }

    [Serializable] // OK
    public class LineColorChanged : IPolygonCommand
    {
        private readonly Color _prevColor, _newColor;

        public LineColorChanged(in Color prevColor, in Color newColor)
        {
            _prevColor = prevColor;
            _newColor = newColor;
        }

        public void Do(in object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            ((ConvexPolygon)obj).LineColor = _newColor;
        }

        public void Undo(in object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            ((ConvexPolygon)obj).LineColor = _prevColor;
        }
    }

    [Serializable] // OK
    public class LineWidthChanged : IPolygonCommand
    {
        private readonly int _prevWidth, _newWidth;

        public LineWidthChanged(in int prevWidth, in int newWidth)
        {
            _prevWidth = prevWidth;
            _newWidth = newWidth;
        }

        public void Do(in object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            ((ConvexPolygon)obj).LineWidth = _newWidth;
        }

        public void Undo(in object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            ((ConvexPolygon)obj).LineWidth = _prevWidth;
        }
    }

    [Serializable] // OK
    public class BackgroundColorChanged : ICommand
    {
        private readonly Color _prevColor, _newColor;

        public BackgroundColorChanged(in Color prevColor, in Color newColor)
        {
            _prevColor = prevColor;
            _newColor = newColor;
        }

        public void Do(in object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            ((Form1)obj).BackColor = _newColor;
        }

        public void Undo(in object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            ((Form1)obj).BackColor = _prevColor;
        }
    }
    /*
    [Serializable]
    public class PluginAdd : ICommand
    {
        private readonly Type _type;
        private readonly int _index;

        public PluginAdd(Type type, int index)
        {
            if (index < 0) throw new ArgumentException();
            _type = type ?? throw new ArgumentNullException("type");
            _index = index;
        }

        public void Do(in object obj)
        {
            var form = (Form1)obj;

            form.
        }

        public void Undo(in object obj);
    }*/
    #endregion
}
