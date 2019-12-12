using Figures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Windows.Forms;

namespace Polygons
{
    public partial class Form1 : Form, IOriginator
    {
        private Type _newFigureShape = typeof(Circle);
        private readonly ConvexPolygon _polygon = new ConvexPolygon();
        private readonly Random _rand = new Random();
        private readonly Timer _dynamicsTimer = new Timer { Interval = 40 };
        
        #region Init
        public Form1()
        {
            InitializeComponent();

            _dynamicsTimer.Tick += (_, __) => Refresh();

            PluginManager.MemorisePlugin(typeof(Circle));

            try
            {
                PluginManager.LoadFromDirectory(Environment.CurrentDirectory + @"\Plugins", "*.dll");
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }

            PlugInit(PluginManager.GetTypes().ToArray());
            (shapeToolStripMenuItem.DropDownItems[0] as ToolStripMenuItem).Checked = true;

            //shapeMenuStrip.PreviewKeyDown += Form1_PreviewKeyDown;
        }

        public ToolStripMenuItem CreateMenuItem(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var newMenuItem = new ToolStripMenuItem(type.Name) { Name = type.Name };

            newMenuItem.Click += (_, __) =>
            {
                #region check mark
                var history = new List<ICommand>();
                foreach (ToolStripMenuItem item in shapeToolStripMenuItem.DropDownItems)
                {
                    if (item.Checked)
                    {
                        item.Checked = false;
                        history.Add(new MenuItemUnchecked(item));
                    }
                }

                var tmpItem = shapeToolStripMenuItem.DropDownItems[shapeToolStripMenuItem.DropDownItems.IndexOf(newMenuItem)] as ToolStripMenuItem;
                tmpItem.Checked = true;
                history.Add(new MenuItemChecked(newMenuItem));

                HistoryManager.Add(history.ToArray());
                #endregion

                _newFigureShape = type;
            };
            return newMenuItem;
        }

        private void PlugInit(params Type[] classes)
        {
            foreach (Type NewClass in classes)
                shapeToolStripMenuItem.DropDownItems.Add(CreateMenuItem(NewClass));
        }
        #endregion

        #region Events
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _dynamicsTimer.Dispose();
            Application.Exit();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z)
            {
                if (e.Shift && HistoryManager.RedoCount > 0)
                    HistoryManager.Redo(this, _polygon);
                else if (!e.Shift && HistoryManager.UndoCount > 0)
                    HistoryManager.Undo(this, _polygon);
                _polygon.ConvexHull();
                Refresh();
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop, false) as string[];
            List<Type> exportedTypes = new List<Type>();

            foreach (string file in files)
            {
                var extension = System.IO.Path.GetExtension(file);
                if (!extension.Equals(".dll", StringComparison.CurrentCultureIgnoreCase))
                {
                    MessageBox.Show("Wrong Plugin: please drop .dll file!");
                    return;
                }
                try
                {
                    exportedTypes.AddRange(PluginManager.LoadFile(file));
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
            }
            foreach (var plugin in exportedTypes)
            {
                if (plugin.FullName.Contains("Figures"))
                {
                    PlugInit(plugin);
                    HistoryManager.Add(new PluginAdd(plugin, plugin.Name)); // !!! already exist error
                }
                else
                {
                    MessageBox.Show("Error: plugin named \"{0}\" is not compartible!", plugin.Name);
                    //PluginManager. // delete plugin
                }
            }
        }
        
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.Link;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            if (_dynamicsTimer.Enabled)
                _polygon.Draw(e.Graphics, _rand);
            else
                _polygon.Draw(e.Graphics);
        }

        private void DynamicsButton_Click(object sender, EventArgs e) // динамика
        {
            _dynamicsTimer.Enabled = !_dynamicsTimer.Enabled;
            //_rand = (_rand ?? new Random());
            dynamicsButton.Refresh();
        }

        private void DynamicsButton_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            Point center = new Point(dynamicsButton.Width / 2, dynamicsButton.Height / 2);
            int r = dynamicsButton.Width / 2 * 6 / 10;
            if (_dynamicsTimer.Enabled)
                e.Graphics.FillRectangle(new SolidBrush(Color.Red), center.X - r, center.Y - r, r * 2, r * 2);
            else
                e.Graphics.FillPolygon(new SolidBrush(Color.Green), new PointF[]
                {
                    new PointF((float)(center.X - Math.Sqrt(3) * r / 2), center.Y - r),
                    new PointF(center.X + r, center.Y),
                    new PointF((float)(center.X - Math.Sqrt(3) * r / 2), center.Y + r)
                });
        }
        
        private static Color ChooseColor(Color StartColor)
        {
            ColorDialog ChooseColorDialog = new ColorDialog
            {
                Color = StartColor,
                FullOpen = true // расширенное окно для выбора цвета
            };


            if (ChooseColorDialog.ShowDialog() == DialogResult.OK)
                StartColor = ChooseColorDialog.Color;
            ChooseColorDialog.Dispose();
            return StartColor;
        }

        #region Mouse events
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            //if (_polygon.DragAndDrop)
            if (DragAndDropManager.Dropping)
            {
                //_polygon.DragAndDropCheck(e.Location);
                DragAndDropManager.Tick(e.Location);
                Refresh();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (DragAndDropManager.Dropping)
            {
                var history = (from i in DragAndDropManager.StartPositions.Zip(DragAndDropManager.DragObjects, (pos, obj) => (pos, obj))
                              select (IPolygonCommand)new VertexPositionChanged(i.pos, i.obj)).ToList();

                DragAndDropManager.Stop();
                var result = _polygon.ConvexHull(history).ToArray();
                if (result.Length > 0)
                    HistoryManager.Add(result);
            }

            Refresh();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var clickedVertices = _polygon.CheckVertices(e.Location);
                if (clickedVertices.Count > 0)
                    DragAndDropManager.Start(e.Location, clickedVertices);
                else if (_polygon.Check(e.Location))
                    DragAndDropManager.Start(e.Location, _polygon.Vertices);
                else
                {
                    var command = new VertexAdd((Vertex)Activator.CreateInstance(_newFigureShape, e.X, e.Y));
                    command.Do(_polygon);
                    HistoryManager.Add(_polygon.ConvexHull(new List<IPolygonCommand>() { command }).ToArray());
                }
            }
            else
            {
                var history = _polygon.RemoveRange(_polygon.CheckVertices(e.Location)).ToArray();
                if (history.Length > 0)
                    HistoryManager.Add(history);
            }

            Refresh();
        }
        #endregion
        #endregion
        // Todo: Undo/Redo for menu items
        // Todo: Undo/Redo for hotplug
        #region Memento
        public void SetMemento(in IMemento memento)
        {
            if (memento == null) throw new ArgumentNullException("memento");

            dynamic state = (memento as Form1Memento).GetState();
            _polygon.SetMemento(state.PolygonMemento as IMemento);
            _newFigureShape = state.Type;
            BackColor = state.BackColor;

            shapeToolStripMenuItem.DropDownItems.Clear();
            shapeToolStripMenuItem.Checked = false;

            PlugInit((state.FigureTypes as IEnumerable<Type>).ToArray());
            (shapeToolStripMenuItem.DropDownItems[state.CheckedMenuItemIndex] as ToolStripMenuItem).Checked = true;
        }

        public IMemento CreateMemento()
        {
            int checkedMenuItemIndex = -1;
            for (int i = 0; i < shapeToolStripMenuItem.DropDownItems.Count && checkedMenuItemIndex == -1; ++i)
            {
                if ((shapeToolStripMenuItem.DropDownItems[i] as ToolStripMenuItem).Checked)
                    checkedMenuItemIndex = i;
            }
            return new Form1Memento(_polygon.CreateMemento() as PolygonMemento, _newFigureShape, BackColor, PluginManager.GetTypes(), checkedMenuItemIndex, null); //, HistoryManager.);
        }
        #endregion

        #region StripMenus
        #region Color
        private void ColorStripMenuVertices_Click(object sender, EventArgs e)
        {
            var NewColor = ChooseColor(_polygon.VertexColor);
            HistoryManager.Add(new VertexColorChanged(_polygon.VertexColor, NewColor));
            _polygon.VertexColor = NewColor;

            Refresh();
        }

        private void ColorStripMenuLines_Click(object sender, EventArgs e)
        {
            var NewColor = ChooseColor(_polygon.LineColor);
            HistoryManager.Add(new LineColorChanged(_polygon.LineColor, NewColor));
            _polygon.LineColor = NewColor;

            Refresh();
        }

        private void ColorStripMenuBackground_Click(object sender, EventArgs e)
        {
            var NewColor = ChooseColor(BackColor);
            HistoryManager.Add(new BackgroundColorChanged(BackColor, NewColor));
            BackColor = NewColor;

            Refresh();
        }
        #endregion
        #region Resize
        private void ResizeStripMenuVertices_Click(object sender, EventArgs e)
        {
            int _originalVertexRadius = _polygon.VertexRadius;
            var ResizeBar = new TrackBar
            {
                Name = "ResizeBar", // Todo: remove?
                Minimum = 5,
                Maximum = 150,
                Value = _polygon.VertexRadius,
                Orientation = Orientation.Horizontal,
                TickStyle = TickStyle.None,
                Location = new Point(0, 0),
                Width = 400
            };
            ResizeBar.Scroll += (_, __) => { _polygon.VertexRadius = ResizeBar.Value; Refresh(); };
            ResizeBar.MouseDown += (_, __) => _originalVertexRadius = _polygon.VertexRadius;
            ResizeBar.MouseUp += (_, __) =>
            {
                if (ResizeBar.Value != _originalVertexRadius)
                    HistoryManager.Add(new VertexRadiusChanged(_originalVertexRadius, ResizeBar.Value));
            };

            var SizeForm = new Form
            {
                ClientSize = new Size(ResizeBar.Width, ResizeBar.Height),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                Text = "Vertices Radius Changer",
                MinimizeBox = false,
                MaximizeBox = false,
                Visible = true,
                KeyPreview = true
            };
            SizeForm.Controls.Add(ResizeBar);
            SizeForm.KeyDown += Form1_KeyDown;
            SizeForm.Deactivate += (_, __) => { SizeForm.Close(); SizeForm.Dispose(); };
        }

        private void ResizeStripMenuLines_Click(object sender, EventArgs e)
        {
            int _originalLineWidth = _polygon.LineWidth;
            var ResizeBar = new TrackBar
            {

                Name = "ResizeBar", // Todo: remove?
                Minimum = 1,
                Maximum = 60,
                Value = _polygon.LineWidth,
                Orientation = Orientation.Horizontal,
                TickStyle = TickStyle.None,
                Location = new Point(0, 0),
                Width = 400,

            };
            ResizeBar.Scroll += (_, __) => { _polygon.LineWidth = ResizeBar.Value; Refresh(); };
            ResizeBar.MouseDown += (_, __) => _originalLineWidth = _polygon.LineWidth;
            ResizeBar.MouseUp += (_, __) =>
            {
                if (ResizeBar.Value != _originalLineWidth)
                    HistoryManager.Add(new LineWidthChanged(_originalLineWidth, ResizeBar.Value));
            };

            var SizeForm = new Form
            {
                ClientSize = new Size(ResizeBar.Width, ResizeBar.Height),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                Text = "Hull Thickness Changer", // Todo: replace with "Lines ..."
                MinimizeBox = false,
                MaximizeBox = false,
                Visible = true,
                KeyPreview = true
            };
            SizeForm.Controls.Add(ResizeBar);
            SizeForm.KeyDown += Form1_KeyDown;
            SizeForm.Deactivate += (_, __) => { SizeForm.Close(); SizeForm.Dispose(); };
        }
        #endregion
        #region Save/Load
        private void FileStripMenuSave_Click(object sender, EventArgs e)
        {
            SaveLoadManager.Save(CreateMemento());
        }

        private void FileStripMenuLoad_Click(object sender, EventArgs e)
        {
            var memento = SaveLoadManager.Load();
            if (memento != null)
                SetMemento(memento as Form1Memento);
        }
        #endregion
        #endregion

        public void SetCheckedMenuItemByHash(in int hashCode, in bool CheckState)
        {
            foreach (ToolStripMenuItem item in shapeToolStripMenuItem.DropDownItems)
                if (item.GetHashCode() == hashCode)
                {
                    item.Checked = CheckState;
                    return;
                }
        }
    }

    [Serializable]
    public class Form1Memento : IMemento
    {

        private readonly PolygonMemento _polygonMemento;
        private readonly Type _type;
        private readonly Color _backColor;
        private readonly IEnumerable<Type> _figureTypes;
        private readonly int _checkedMenuItemIndex;
        private readonly IEnumerable<ICommand> _history;

        public Form1Memento(in PolygonMemento polygonMemento, in Type type, in Color backColor, in IEnumerable<Type> figureTypes, in int checkedMenuItemIndex, in IEnumerable<ICommand> history)
        {
            _polygonMemento = polygonMemento ?? throw new ArgumentNullException(nameof(polygonMemento));
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _figureTypes = figureTypes ?? throw new ArgumentNullException(nameof(figureTypes));
            _backColor = backColor;
            _checkedMenuItemIndex = checkedMenuItemIndex;
            _history = history;
        }

        public ExpandoObject GetState()
        {
            dynamic state = new ExpandoObject();

            state.PolygonMemento = _polygonMemento;
            state.Type = _type;
            state.BackColor = _backColor;
            state.FigureTypes = _figureTypes;
            state.CheckedMenuItemIndex = _checkedMenuItemIndex;
            state.History = _history;

            return state;
        }
    }
}
