using Figures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Polygons
{
    public interface ICommand
    {
        void Do();
        void Undo();
    }

    [Serializable]
    public static class HistoryManager //: IOriginator
    {
        private static Stack<IEnumerable<ICommand>> _forUndo, _forRedo;
        public static int UndoCount => _forUndo.Count;
        public static int RedoCount => _forRedo.Count;

        static HistoryManager()
        {
            _forUndo = new Stack<IEnumerable<ICommand>>();
            _forRedo = new Stack<IEnumerable<ICommand>>();
        }

        public static void Add(params ICommand[] item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item.Length == 0) throw new ArgumentException();

            _forUndo.Push(item);
            if (RedoCount > 0 && _forRedo.Peek() != item) _forRedo.Clear();
        }

        public static void Undo(in Form1 form, in ConvexPolygon polygon)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            if (polygon == null) throw new ArgumentNullException(nameof(polygon));
            if (UndoCount < 1) throw new InvalidOperationException("Operation is not valid as there is nothing to undo.");

            foreach (ICommand command in (_forUndo.Peek()).Reverse())
            {
                if (command is IPolygonCommand)
                    command.Undo(polygon);
                else
                    command.Undo(form);
            }

            _forRedo.Push(_forUndo.Pop());
        }

        public static void Redo(in Form1 form, in ConvexPolygon polygon)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            if (polygon == null) throw new ArgumentNullException(nameof(polygon));
            if (RedoCount < 1) throw new InvalidOperationException("Operation is not valid as there is nothing to redo.");

            foreach (ICommand command in _forRedo.Peek())
            {
                if (command is IPolygonCommand)
                    command.Do(polygon);
                else
                    command.Do(form);
            }

            _forUndo.Push(_forRedo.Pop());
        }

        //Clear method

        #region Memento
        /*
        public static IMemento CreateMemento()
        {
            return new HistoryManagerMemento(_forUndo, _forRedo) as IMemento;
        }

        public static void SetMemento(in IMemento memento)
        {
            dynamic state = (memento as HistoryManagerMemento).GetState();

            _forRedo = state.ForRedo;
            _forUndo = state.ForUndo;
        }
        */
        #endregion
    }

    /*
    public class HistoryManagerMemento : IMemento
    {
        private readonly Stack<IEnumerable<ICommand>> _forUndo, _forRedo;

        public HistoryManagerMemento(in Stack<IEnumerable<ICommand>> forUndo, in Stack<IEnumerable<ICommand>> forRedo)
        {
            _forUndo = forUndo ?? throw new ArgumentNullException(nameof(forUndo));
            _forRedo = forRedo ?? throw new ArgumentNullException(nameof(forRedo));
        }

        public ExpandoObject GetState()
        {
            dynamic state = new ExpandoObject();

            state.ForUndo = _forUndo;
            state.ForRedo = _forRedo;

            return state;
        }
    }
    */

    #region Commands
    /*
[Serializable]
public abstract class Command : IEquatable<Command>, ICommand
{
    protected readonly object _linkToObject;

    protected Command(in object obj)
    {
        _linkToObject = obj ?? throw new ArgumentNullException(nameof(obj));
    }

    public abstract void Do();

    public abstract void Undo();

    public override bool Equals(object obj)
    {
        return Equals(obj as Command);
    }

    public override abstract int GetHashCode();

    public abstract bool Equals(Command other);
}

[Serializable]
public sealed class VertexAdd : Command
{
    private readonly Vertex _item;

    public VertexAdd(in Vertex item, in ConvexPolygon polygon) : base(polygon)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
    }

    public override void Do()
    {
        (_linkToObject as ConvexPolygon).Add(_item);
    }

    public override void Undo()
    {
        (_linkToObject as ConvexPolygon).Remove(_item);
    }

    public override bool Equals(Command other)
    {
        return _item.Equals(other._item);
    }

    public override int GetHashCode()
    {
        int hashcode = _linkToObject.GetHashCode();
        hashcode = 31 * hashcode + _item.GetHashCode();
        return hashcode;
    }
}
*/

    [Serializable]
    public sealed class VertexAdd : ICommand
    {
        private readonly ConvexPolygon _polygon;
        private readonly Vertex _item;

        public VertexAdd(in ConvexPolygon polygon, in Vertex item)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));
            _polygon = polygon ?? throw new ArgumentNullException(nameof(polygon));
        }

        public void Do()
        {
            _polygon.Add(_item);
        }

        public void Undo()
        {
            _polygon.Remove(_item);
        }
    }

    [Serializable]
    public sealed class VertexDelete : ICommand
    {
        private readonly ConvexPolygon _polygon;
        private readonly Vertex _item;

        public VertexDelete(in ConvexPolygon polygon, in Vertex item)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));
            _polygon = polygon ?? throw new ArgumentNullException(nameof(polygon));
        }

        public void Do()
        {
            _polygon.Remove(_item);
        }

        public void Undo()
        {
            _polygon.Add(_item);
        }
    }

    [Serializable]
    public sealed class VertexColorChanged : ICommand
    {
        private readonly ConvexPolygon _polygon;
        private readonly Color _newColor, _prevColor;

        public VertexColorChanged(in ConvexPolygon polygon, in Color prevColor, in Color newColor)
        {
            _polygon = polygon ?? throw new ArgumentNullException(nameof(polygon));
            _prevColor = prevColor;
            _newColor = newColor;
        }

        public void Do()
        {
            _polygon.VertexColor = _newColor;
        }

        public void Undo()
        {
            _polygon.VertexColor = _prevColor;
        }
    }

    [Serializable]
    public sealed class VertexRadiusChanged : ICommand
    {
        private readonly ConvexPolygon _polygon;
        private readonly int _prevRadius, _newRadius;

        public VertexRadiusChanged(in ConvexPolygon polygon, in int prevRadius, in int newRadius)
        {
            _polygon = polygon ?? throw new ArgumentNullException(nameof(polygon));
            _prevRadius = prevRadius;
            _newRadius = newRadius;
        }

        public void Do()
        {
            _polygon.VertexRadius = _newRadius;
        }

        public void Undo()
        {
            _polygon.VertexRadius = _prevRadius;
        }

    }

    [Serializable]
    public sealed class VertexPositionChanged : ICommand
    {
        private readonly Vertex _vertex;
        private readonly Point _prevPosition, _newPosition;

        public VertexPositionChanged(in Vertex item, in Point newPosition)
        {
            _vertex = item ?? throw new ArgumentNullException(nameof(item));
            _newPosition = newPosition;
            _prevPosition = new Point(item.X, item.Y);
        }

        public VertexPositionChanged(in Point previousPosition, in Vertex item)
        {
            _vertex = item ?? throw new ArgumentNullException(nameof(item));
            _prevPosition = previousPosition;
            _newPosition = new Point(item.X, item.Y);
        }

        public void Do()
        {
            _vertex.X = _newPosition.X;
            _vertex.Y = _newPosition.Y;
        }

        public void Undo()
        {
            _vertex.X = _prevPosition.X;
            _vertex.Y = _prevPosition.Y;
        }
    }

    [Serializable]
    public sealed class LineColorChanged : ICommand
    {
        private readonly ConvexPolygon _polygon;
        private readonly Color _prevColor, _newColor;

        public LineColorChanged(in ConvexPolygon polygon, in Color prevColor, in Color newColor)
        {
            _polygon = polygon ?? throw new ArgumentNullException(nameof(polygon));
            _prevColor = prevColor;
            _newColor = newColor;
        }

        public void Do()
        {
            _polygon.LineColor = _newColor;
        }

        public void Undo()
        {
            _polygon.LineColor = _prevColor;
        }
    }

    [Serializable]
    public sealed class LineWidthChanged : ICommand
    {
        private readonly ConvexPolygon _polygon;
        private readonly int _prevWidth, _newWidth;

        public LineWidthChanged(in ConvexPolygon polygon, in int prevWidth, in int newWidth)
        {
            _polygon = polygon ?? throw new ArgumentNullException(nameof(polygon));
            _prevWidth = prevWidth;
            _newWidth = newWidth;
        }

        public void Do()
        {
            _polygon.LineWidth = _newWidth;
        }

        public void Undo()
        {
            _polygon.LineWidth = _prevWidth;
        }
    }

    [Serializable]
    public sealed class BackgroundColorChanged : ICommand
    {
        private readonly Form _form;
        private readonly Color _prevColor, _newColor;

        public BackgroundColorChanged(in Form form, in Color prevColor, in Color newColor)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
            _prevColor = prevColor;
            _newColor = newColor;
        }

        public void Do()
        {
            _form.BackColor = _newColor;
        }

        public void Undo()
        {
            _form.BackColor = _prevColor;
        }
    }

    // Todo: [Serializable]
    public abstract class MenuAction
    {
        private readonly Form1 _form;
        private readonly int _hashCode;

        protected MenuAction(in Form1 form, in ToolStripMenuItem item)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
            if (item == null) throw new ArgumentNullException(nameof(item));
            _hashCode = item.GetHashCode();
        }

        private static ToolStripMenuItem FindMenuItem(in ToolStripItemCollection items, in int hashCode)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            foreach (ToolStripMenuItem item in items)
                if (item.GetHashCode() == hashCode)
                    return item;
            return null;
        }

        protected void Check()
        {
            var item = FindMenuItem(_form.GetMenuItems(), _hashCode) ?? throw new InvalidOperationException();
            item.Checked = true;
        }

        protected void Uncheck()
        {
            var item = FindMenuItem(_form.GetMenuItems(), _hashCode) ?? throw new InvalidOperationException();
            item.Checked = false;
        }
    }

    [Serializable]
    public sealed class MenuItemChecked : ICommand
    {
        private readonly int _hashCode;
        public MenuItemChecked(int hashCode) => _hashCode = hashCode;

        public void Do() => Check();

        public void Undo() => Uncheck();
    }

    [Serializable]
    public sealed class MenuItemUnchecked : MenuAction, ICommand
    {
        public MenuItemUnchecked(in Form1 form, in ToolStripMenuItem item) : base(form, item) { }

        public void Do() => Uncheck();

        public void Undo() => Check();
    }

    [Serializable]
    public sealed class PluginAdd : ICommand
    {
        private readonly Type _type;
        private readonly string _menuItemName;

        public PluginAdd(in Type type, in string menuItemName)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _menuItemName = menuItemName ?? throw new ArgumentNullException(nameof(menuItemName));
        }

        public void Do(in object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            (obj as Form1).GetMenuItems().Add((obj as Form1).CreateMenuItem(_type));
        }

        public void Undo(in object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            var items = (obj as Form1).GetMenuItems();
            items.Remove(items.Find(_menuItemName, false)[0]);
        }
    }
    
    // Todo: public class PluginAdd
    #endregion
}
