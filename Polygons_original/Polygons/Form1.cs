using Figures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Polygons
{
    public partial class Form1 : Form, IOriginator
    {
        private Type _newFigureShape = typeof(Circle);
        private ConvexPolygon _polygon = new ConvexPolygon();
        private readonly Random _rand = new Random();
        private readonly Timer _dynamicsTimer = new Timer { Interval = 40 };
        
        #region Init
        public Form1()
        {
            InitializeComponent();
            AllowDrop = true;
            KeyPreview = true;
            //ConvexPolygon.ColorChanged += (_, __) => Refresh();
            //ConvexPolygon.WidthChanged += (_, __) => Refresh();

            #region Filed init
            #endregion

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
            ((ToolStripMenuItem)shapeToolStripMenuItem.DropDownItems[0]).Checked = true;
        }

        private void PlugInit(params Type[] classes)
        {
            foreach (var newClass in classes)
            {
                var newMenuItem = new ToolStripMenuItem(newClass.Name);

                newMenuItem.Click += (_, __) =>
                {

                    #region check mark
                    foreach (ToolStripMenuItem item in shapeToolStripMenuItem.DropDownItems)
                        item.Checked = false;

                    var tmp = (ToolStripMenuItem)shapeToolStripMenuItem.DropDownItems[shapeToolStripMenuItem.DropDownItems.IndexOf(newMenuItem)];
                    tmp.Checked = true;
                    #endregion

                    _newFigureShape = newClass;
                };

                shapeToolStripMenuItem.DropDownItems.Add(newMenuItem);
            }
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
            if (e.Control)
            {
                if (e.KeyCode == Keys.Z)
                {
                    if (e.Shift && HistoryManager.RedoCount > 0)
                        HistoryManager.Redo(this, _polygon);
                    else if (!e.Shift && HistoryManager.UndoCount > 0)
                        HistoryManager.Undo(this, _polygon);
                    _polygon.ConvexHull();
                    Refresh();
                }
            }
        }
        // Todo: undo/redo for drag&drop
        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            var exportedTypes = new List<Type>();

            foreach (var file in files)
            {
                var extension = System.IO.Path.GetExtension(file);
                if (!extension.Equals(".dll", StringComparison.CurrentCultureIgnoreCase))
                {
                    MessageBox.Show(@"Wrong Plugin: please drop .dll file!");
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
            PlugInit(exportedTypes.ToArray());
        }
        /*
        private int GetCheckedIndex()
        {
            int index = -1;
            System.Collections.IList list = shapeToolStripMenuItem.DropDownItems;
            for (int i = 0; i < list.Count && index == -1; i++)
            {
                ToolStripMenuItem item = (ToolStripMenuItem)list[i];
                if (item.Checked)
                    index = i;
            }
            return index;
        }
        */
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
            dynamicsButton.Refresh();
        }

        private void DynamicsButton_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            var center = new Point(dynamicsButton.Width / 2, dynamicsButton.Height / 2);
            var r = dynamicsButton.Width / 2 * 6 / 10;
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
        
        private static Color ChooseColor(Color startColor)
        {
            var chooseColorDialog = new ColorDialog
            {
                Color = startColor,
                FullOpen = true // расширенное окно для выбора цвета
            };


            if (chooseColorDialog.ShowDialog() == DialogResult.OK)
                startColor = chooseColorDialog.Color;
            chooseColorDialog.Dispose();
            return startColor;
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
        #region Memento
        public void SetMemento(in IMemento memento)
        {
            if (memento == null) return;
            //if (_memento == null) throw new ArgumentNullException("_memento");

            dynamic data = ((Form1Memento)memento).GetState();

            _polygon = data.Polygon;
            _newFigureShape = data.Type;
            BackColor = data.BackColor;

            shapeToolStripMenuItem.DropDownItems.Clear();
            shapeToolStripMenuItem.Checked = false;

            PlugInit(((IEnumerable<Type>)data.FigureTypes).ToArray());
            ((ToolStripMenuItem)shapeToolStripMenuItem.DropDownItems[data.CheckedIndex]).Checked = true;
        }

        public IMemento CreateMemento()
        {
            var checkedIndex = -1;
            for (var i = 0; i < shapeToolStripMenuItem.DropDownItems.Count && checkedIndex == -1; ++i)
            {
                if (((ToolStripMenuItem)shapeToolStripMenuItem.DropDownItems[i]).Checked)
                    checkedIndex = i;
            }
            return new Form1Memento(_polygon, _newFigureShape, BackColor, PluginManager.GetTypes(), checkedIndex);
        }
        #endregion

        #region StripMenus
        #region Color
        private void ColorStripMenuVertices_Click(object sender, EventArgs e)
        {
            var newColor = ChooseColor(_polygon.VertexColor);
            HistoryManager.Add(new VertexColorChanged(_polygon.VertexColor, newColor));
            _polygon.VertexColor = newColor;

            Refresh();
        }

        private void ColorStripMenuLines_Click(object sender, EventArgs e)
        {
            var newColor = ChooseColor(_polygon.LineColor);
            HistoryManager.Add(new LineColorChanged(_polygon.LineColor, newColor));
            _polygon.LineColor = newColor;

            Refresh();
        }

        private void ColorStripMenuBackground_Click(object sender, EventArgs e) // Todo: add undo/redo
        {
            var newColor = ChooseColor(BackColor);
            HistoryManager.Add(new BackgroundColorChanged(BackColor, newColor));
            BackColor = newColor;

            Refresh();
        }
        #endregion
        #region Resize
        private void ResizeStripMenuVertices_Click(object sender, EventArgs e)
        {
            var originalVertexRadius = _polygon.VertexRadius;
            var resizeBar = new TrackBar
            {
                Minimum = 5,
                Maximum = 150,
                Value = _polygon.VertexRadius,
                Orientation = Orientation.Horizontal,
                TickStyle = TickStyle.None,
                Location = new Point(0, 0),
                Width = 400
            };
            resizeBar.Scroll += (_, __) => { _polygon.VertexRadius = resizeBar.Value; Refresh(); };
            resizeBar.MouseDown += (_, __) => originalVertexRadius = _polygon.VertexRadius;
            resizeBar.MouseUp += (_, __) =>
            {
                if (resizeBar.Value != originalVertexRadius)
                    HistoryManager.Add(new VertexRadiusChanged(originalVertexRadius, resizeBar.Value));
            };

            var sizeForm = new Form
            {
                ClientSize = new Size(resizeBar.Width, resizeBar.Height),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                MinimizeBox = false,
                MaximizeBox = false,
                Visible = true,
                KeyPreview = true
            };
            sizeForm.Controls.Add(resizeBar);
            sizeForm.KeyDown += Form1_KeyDown;
            sizeForm.Deactivate += (_, __) => { sizeForm.Close(); sizeForm.Dispose(); };
        }

        private void ResizeStripMenuLines_Click(object sender, EventArgs e)
        {
            var originalLineWidth = _polygon.LineWidth;
            var resizeBar = new TrackBar
            {
                Minimum = 1,
                Maximum = 60,
                Value = _polygon.LineWidth,
                Orientation = Orientation.Horizontal,
                TickStyle = TickStyle.None,
                Location = new Point(0, 0),
                Width = 400,

            };
            resizeBar.Scroll += (_, __) => { _polygon.LineWidth = resizeBar.Value; Refresh(); };
            resizeBar.MouseDown += (_, __) => originalLineWidth = _polygon.LineWidth;
            resizeBar.MouseUp += (_, __) =>
            {
                if (resizeBar.Value != originalLineWidth)
                    HistoryManager.Add(new LineWidthChanged(originalLineWidth, resizeBar.Value));
            };

            var sizeForm = new Form
            {
                ClientSize = new Size(resizeBar.Width, resizeBar.Height),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                MinimizeBox = false,
                MaximizeBox = false,
                Visible = true,
                KeyPreview = true
            };
            sizeForm.Controls.Add(resizeBar);
            sizeForm.KeyDown += Form1_KeyDown;
            sizeForm.Deactivate += (_, __) => { sizeForm.Close(); sizeForm.Dispose(); };
        }
        #endregion
        #region Save/Load
        private void FileStripMenuSave_Click(object sender, EventArgs e)
        {
            SaveLoadManager.Save(CreateMemento());
        }

        private void FileStripMenuLoad_Click(object sender, EventArgs e)
        {
            SetMemento(SaveLoadManager.Load());
        }
        #endregion
        #endregion
    }
}
