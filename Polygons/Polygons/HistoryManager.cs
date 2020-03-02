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

        public static void Add(IList<ICommand> history)
        {
            if (history is null) throw new ArgumentNullException(nameof(history));
            if (history.Count == 0) throw new ArgumentException();

            ForUndo.Push(history);
            if (RedoCount > 0 && ForRedo.Peek() != history) ForRedo.Clear();
        }

        public static void Undo()
        {
            if (UndoCount < 1) throw new InvalidOperationException("Operation is not valid as there is nothing to undo.");

            foreach (var command in ForUndo.Peek().Reverse())
                command.Undo();

            ForRedo.Push(ForUndo.Pop());
        }

        public static void Redo()
        {
            if (RedoCount < 1) throw new InvalidOperationException("Operation is not valid as there is nothing to redo.");

            foreach (var command in ForRedo.Peek())
                command.Do();

            ForUndo.Push(ForRedo.Pop());
        }

    }

    #region Commands

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

    public sealed class MenuItemChecked : ICommand
    {
        private readonly ToolStripMenuItem _item;
        public MenuItemChecked(in ToolStripMenuItem item)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));
        }

        public void Do()
        {
            _item.Checked = true;
        }

        public void Undo()
        {
            _item.Checked = false;
        }
    }

    public sealed class MenuItemUnchecked : ICommand
    {
        private readonly ToolStripMenuItem _item;
        public MenuItemUnchecked(in ToolStripMenuItem item)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));
        }

        public void Do()
        {
            _item.Checked = false;
        }

        public void Undo()
        {
            _item.Checked = true;
        }
    }
    
    public sealed class PluginAdd : ICommand
    {
        private readonly ToolStripMenuItem _item;
        private readonly ToolStripItemCollection _parent;

        public PluginAdd(in ToolStripMenuItem item, in ToolStripItemCollection itemCollection)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));
            _parent = itemCollection ?? throw new ArgumentNullException(nameof(itemCollection));
        }

        public void Do()
        {
            _parent.Add(_item);
        }

        public void Undo()
        {
            _parent.Remove(_item);
        }
    }

    public sealed class Load : ICommand
    {
        private readonly MainControl _form;
        private readonly MainControlMemento _prevState, _newState;

        public Load(in MainControl form, in MainControlMemento prevState, in MainControlMemento newState)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
            _prevState = prevState ?? throw new ArgumentNullException(nameof(prevState));
            _newState = newState ?? throw new ArgumentNullException(nameof(newState));
        }

        public void Do()
        {
            _form.SetMemento(_newState); 
        }

        public void Undo()
        {
            _form.SetMemento(_prevState);
        }
    }
    
    #endregion
}
