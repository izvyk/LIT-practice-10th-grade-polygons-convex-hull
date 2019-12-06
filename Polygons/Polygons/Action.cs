using System;
using System.Collections.Generic;

namespace Polygons
{
    interface IAction { }

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
}
