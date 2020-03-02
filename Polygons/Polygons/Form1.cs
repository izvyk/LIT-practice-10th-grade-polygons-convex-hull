using Figures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Polygons
{
    public partial class MainControl : Form, IOriginator
    {
        private Type _newFigureShape = typeof(Circle);
        private readonly ConvexPolygon _polygon = new ConvexPolygon();
        private readonly Random _rand = new Random();
        private readonly Timer _dynamicsTimer = new Timer { Interval = 40 };
        private readonly List<Type> _pluginList = new List<Type>();
        private readonly Form _verticesResizeForm, _linesResizeForm;
        
        #region Init
        public MainControl()
        {
            InitializeComponent();

            _verticesResizeForm = InitVerticesResizeForm();
            _linesResizeForm = InitLinesResizeForm();

            _dynamicsTimer.Tick += (_, __) => Invalidate();

            PlugInit(typeof(Circle));
            ((ToolStripMenuItem) shapeToolStripMenuItem.DropDownItems[0]).Checked = true;

            try
            {
                PlugInit(PluginManager.LoadFromDirectory(Environment.CurrentDirectory + @"\Plugins", "*.dll").Distinct().ToArray());
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

        }

        private Form InitVerticesResizeForm()
        {
            int originalVertexRadius = _polygon.VertexRadius;
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
            ResizeBar.Scroll += (_, __) => { _polygon.VertexRadius = ResizeBar.Value; Invalidate(); };
            ResizeBar.MouseDown += (_, __) => originalVertexRadius = _polygon.VertexRadius;
            ResizeBar.MouseUp += (_, __) =>
            {
                if (ResizeBar.Value != originalVertexRadius)
                    HistoryManager.Add(new VertexRadiusChanged(_polygon, originalVertexRadius, ResizeBar.Value));
            };

            var SizeForm = new Form
            {
                ClientSize = new Size(ResizeBar.Width, ResizeBar.Height),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                Text = "Vertices Radius Changer",
                MinimizeBox = false,
                MaximizeBox = false,
                Visible = false,
                KeyPreview = true
            };
            SizeForm.Controls.Add(ResizeBar);
            SizeForm.KeyDown += Form1_KeyDown;
            SizeForm.FormClosing += (_, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    Hide();
                }
            };
            SizeForm.VisibleChanged += (_, __) => ResizeBar.Value = _polygon.VertexRadius;

            return SizeForm;
        }

        private Form InitLinesResizeForm()
        {

            var originalLineWidth = _polygon.LineWidth;
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
            ResizeBar.Scroll += (_, __) => { _polygon.LineWidth = ResizeBar.Value; Invalidate(); };
            ResizeBar.MouseDown += (_, __) => originalLineWidth = _polygon.LineWidth;
            ResizeBar.MouseUp += (_, __) =>
            {
                if (ResizeBar.Value != originalLineWidth)
                    HistoryManager.Add(new LineWidthChanged(_polygon, originalLineWidth, ResizeBar.Value));
            };

            var SizeForm = new Form
            {
                ClientSize = new Size(ResizeBar.Width, ResizeBar.Height),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                Text = "Hull Thickness Changer", // Todo: replace with "Lines ..."
                MinimizeBox = false,
                MaximizeBox = false,
                Visible = false,
                KeyPreview = true
            };
            SizeForm.Controls.Add(ResizeBar);
            SizeForm.KeyDown += Form1_KeyDown;
            SizeForm.FormClosing += (_, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    Hide();
                }
            };
            SizeForm.VisibleChanged += (_, __) => ResizeBar.Value = _polygon.LineWidth;

            return SizeForm;
        }

        private ToolStripMenuItem CreateMenuItem(Type type)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            var newMenuItem = new ToolStripMenuItem(type.Name) { Name = type.Name };

            newMenuItem.Click += (_, __) =>
            {
                #region check mark
                var history = new List<ICommand>();
                foreach (ToolStripMenuItem item in shapeToolStripMenuItem.DropDownItems)
                {
                    if (!item.Checked) continue;
                    item.Checked = false;
                    history.Add(new MenuItemUnchecked(item));
                }

                newMenuItem.Checked = true;
                history.Add(new MenuItemChecked(newMenuItem));

                HistoryManager.Add(history);
                #endregion

                _newFigureShape = type;
            };

            return newMenuItem;
        }
        /*foreach (var i in exportedTypes)
                {
                    if (ExportedTypes.Contains(i))
                        throw new Exception($"Error while adding plugin: plugin named \"{i.Name}\" already exists!");
                }*/

        private void PlugInit(params Type[] classes)
        {
            foreach (var newClass in classes.Distinct())
            {
                if (!_pluginList.Contains(newClass))
                {
                    _pluginList.Add(newClass);
                    shapeToolStripMenuItem.DropDownItems.Add(CreateMenuItem(newClass));
                }
            }
        }
        #endregion

        #region Events
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _dynamicsTimer.Dispose();
            Application.Exit();
        }
        // Todo: cansel Dnd on Esc
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z && !DragAndDropManager.Dropping)
            {
                if (e.Shift && HistoryManager.RedoCount > 0)
                    HistoryManager.Redo();
                else if (!e.Shift && HistoryManager.UndoCount > 0)
                    HistoryManager.Undo();
                //_polygon.MakeConvex(); Todo: convex
                Invalidate();
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e) // Todo: add folder drop
        {
            var files = e.Data.GetData(DataFormats.FileDrop, false) as string[] ?? throw new Exception();
            var exportedTypes = new List<Type>();

            foreach (var file in files)
            {
                var extension = System.IO.Path.GetExtension(file);
                if (!extension.Equals(".dll", StringComparison.CurrentCultureIgnoreCase))
                {
                    MessageBox.Show($@"Wrong Plugin: file is not a plugin and will be skipped!\nFile: ""{ file }""");
                    continue;
                }
                exportedTypes.AddRange(PluginManager.LoadFile(file));
            }

            for (var index = 0; index < exportedTypes.Count; ++index)
            {
                var plugin = exportedTypes[index];
                if (plugin.FullName.Contains("Figures"))
                {
                    var newItem = CreateMenuItem(plugin);
                    var command = new PluginAdd(newItem, shapeToolStripMenuItem.DropDownItems);
                    command.Do();
                    HistoryManager.Add(command); // !!! already exist error
                }
                else if (_pluginList.Contains(plugin))
                    MessageBox.Show(
                        $@"Error while adding plugin: plugin named ""{plugin.Name}"" already exists, it will be skipped.");
                else
                {
                    MessageBox.Show($@"Error: plugin named ""{plugin.Name}"" is not compatible!");
                    exportedTypes.RemoveAt(index--);
                }
            }
        }
        
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.Link;
        }

        // Todo: Drag and Drop file extension check

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            if (_dynamicsTimer.Enabled)
                foreach (var i in _polygon.Vertices)
                {
                    i.X += _rand.Next(-1, 2);
                    i.Y += _rand.Next(-1, 2);
                }
            _polygon.Draw(e.Graphics);
        }

        private void DynamicsButton_Click(object sender, EventArgs e)
        {
            _dynamicsTimer.Enabled = !_dynamicsTimer.Enabled;
            dynamicsButton.Invalidate();
        }

        private void DynamicsButton_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            var center = new Point(dynamicsButton.Width / 2, dynamicsButton.Height / 2);
            var r = dynamicsButton.Width / 2 * 6 / 10;
            if (_dynamicsTimer.Enabled)
                e.Graphics.FillRectangle(new SolidBrush(Color.Red), center.X - r, center.Y - r, r * 2, r * 2);
            else
                e.Graphics.FillPolygon(new SolidBrush(Color.Green), new []
                {
                    new PointF((float)(center.X - Math.Sqrt(3) * r / 2), center.Y - r),
                    new PointF(center.X + r, center.Y),
                    new PointF((float)(center.X - Math.Sqrt(3) * r / 2), center.Y + r)
                });
        }
        
        private static Color ChooseColor(Color startColor)
        {
            using (var ChooseColorDialog = new ColorDialog {Color = startColor, FullOpen = true})
                if (ChooseColorDialog.ShowDialog() == DialogResult.OK)
                    startColor = ChooseColorDialog.Color;

            return startColor;
        }

        #region Mouse events
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!DragAndDropManager.Dropping) return;
            DragAndDropManager.Step(e.Location);
            Invalidate();
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (!DragAndDropManager.Dropping) return;
            var history = new List<ICommand>(
                from i in DragAndDropManager.StartPositions.Zip(DragAndDropManager.DragObjects, (pos, obj) => (pos, obj))
                select (ICommand)new VertexPositionChanged(i.pos, i.obj));

            DragAndDropManager.Stop();


            //var result = new List<ICommand>(_polygon.MakeConvex(history));
            //if (result.Count > 0)
            //    HistoryManager.Add(result);
            // Todo: history
            Invalidate();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var clickedVertices = _polygon.CheckVertices(e.Location);
                if (clickedVertices.Any())
                    DragAndDropManager.Start(e.Location, clickedVertices.ToList());
                else if (_polygon.Contains(e.Location))
                    DragAndDropManager.Start(e.Location, _polygon.Vertices);
                else
                {
                    var history = new List<ICommand> { new VertexAdd(_polygon, (Vertex)Activator.CreateInstance(_newFigureShape, e.X, e.Y)) };
                    history[0].Do();
                    var vertices = _polygon.Vertices;
                    _polygon.MakeConvex();
                    history.AddRange(
                        from i in vertices.Except(_polygon.Vertices)
                        select new VertexDelete(_polygon, i));
                    HistoryManager.Add(history);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                var history = _polygon.CheckVertices(e.Location).Select(vertex => new VertexDelete(_polygon, vertex)).Cast<ICommand>().ToList();

                if (history.Count > 0)
                {
                    history.ForEach(i => i.Do());
                    HistoryManager.Add(history);
                }
            }

            Invalidate();
        }
        #endregion
        #endregion
        // Todo: Undo/Redo for menu items
        // Todo: Undo/Redo for hotplug
        #region Memento
        public void SetMemento(in IMemento memento)
        {
            if (memento == null) throw new ArgumentNullException(nameof(memento));

            dynamic state = (memento as MainControlMemento).GetState();
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
            var checkedMenuItemIndex = -1;
            for (var i = 0; i < shapeToolStripMenuItem.DropDownItems.Count && checkedMenuItemIndex == -1; ++i)
            {
                if ((shapeToolStripMenuItem.DropDownItems[i] as ToolStripMenuItem).Checked)
                    checkedMenuItemIndex = i;
            }
            return new MainControlMemento(_polygon.CreateMemento() as PolygonMemento, _newFigureShape, BackColor, _pluginList, checkedMenuItemIndex); //, HistoryManager.);
        }
        #endregion

        #region StripMenus
        #region Color
        private void ColorStripMenuVertices_Click(object sender, EventArgs e)
        {
            var NewColor = ChooseColor(_polygon.VertexColor);
            HistoryManager.Add(new VertexColorChanged(_polygon, _polygon.VertexColor, NewColor));
            _polygon.VertexColor = NewColor;

            Invalidate();
        }

        private void ColorStripMenuLines_Click(object sender, EventArgs e)
        {
            var NewColor = ChooseColor(_polygon.LineColor);
            HistoryManager.Add(new LineColorChanged(_polygon, _polygon.LineColor, NewColor));
            _polygon.LineColor = NewColor;

            Invalidate();
        }

        private void ColorStripMenuBackground_Click(object sender, EventArgs e)
        {
            var NewColor = ChooseColor(BackColor);
            HistoryManager.Add(new BackgroundColorChanged(this, BackColor, NewColor));
            BackColor = NewColor;

            Invalidate();
        }
        #endregion
        #region Resize
        private void ResizeStripMenuVertices_Click(object sender, EventArgs e)
        {
            _verticesResizeForm.Visible = true;
        }

        private void ResizeStripMenuLines_Click(object sender, EventArgs e)
        {
            _linesResizeForm.Visible = true;
        }
        #endregion
        #region Save/Load
        private void FileStripMenuSave_Click(object sender, EventArgs e)
        {
            SaveLoadManager.Save(CreateMemento());
        }

        private void FileStripMenuLoad_Click(object sender, EventArgs e)
        {
            var prevState = CreateMemento();
            IMemento newState = null;
            try
            {
                newState = SaveLoadManager.Load()[0] as MainControlMemento;
            }
            catch (DllNotFoundException)
            {
                MessageBox.Show(@"Unable to find a plugin!");
            }
            if (newState is null || newState.Equals(prevState)) return;

            SetMemento(newState);
            HistoryManager.Add(new Load(this,
                    prevState as MainControlMemento,
                    newState as MainControlMemento));
        }
        #endregion
        #endregion
    }

    [Serializable]
    public class MainControlMemento : IMemento
    {
        private readonly PolygonMemento _polygonMemento;
        private readonly Type _type;
        private readonly Color _backColor;
        private readonly IEnumerable<Type> _figureTypes;
        private readonly int _checkedMenuItemIndex;

        public MainControlMemento(in PolygonMemento polygonMemento, in Type type, in Color backColor, in IEnumerable<Type> figureTypes, in int checkedMenuItemIndex)
        {
            _polygonMemento = polygonMemento ?? throw new ArgumentNullException(nameof(polygonMemento));
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _figureTypes = figureTypes ?? throw new ArgumentNullException(nameof(figureTypes));
            _backColor = backColor;
            _checkedMenuItemIndex = checkedMenuItemIndex;
        }

        public ExpandoObject GetState()
        {
            dynamic state = new ExpandoObject();

            state.PolygonMemento = _polygonMemento;
            state.Type = _type;
            state.BackColor = _backColor;
            state.FigureTypes = _figureTypes;
            state.CheckedMenuItemIndex = _checkedMenuItemIndex;

            return state;
        }
    }
}
