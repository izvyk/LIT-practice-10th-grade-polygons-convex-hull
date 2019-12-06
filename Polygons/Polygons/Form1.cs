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
        private int? Radius = null, Lines = null;
        
        #region Init
        public Form1()
        {
            InitializeComponent();
            AllowDrop = true;
            KeyPreview = true;
            ConvexPolygon.ColorChanged += (_, __) => Refresh();
            ConvexPolygon.WidthChanged += (_, __) => Refresh();

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
            foreach (Type NewClass in classes)
            {
                var newMenuItem = new ToolStripMenuItem(NewClass.Name);

                newMenuItem.Click += (_, __) =>
                {

                    #region check mark
                    foreach (ToolStripMenuItem item in shapeToolStripMenuItem.DropDownItems)
                        item.Checked = false;

                    ToolStripMenuItem tmp = (ToolStripMenuItem)shapeToolStripMenuItem.DropDownItems[shapeToolStripMenuItem.DropDownItems.IndexOf(newMenuItem)];
                    tmp.Checked = true;
                    #endregion

                    _newFigureShape = NewClass;
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
                    if (e.Shift)
                        _polygon.Redo();
                    else
                        _polygon.Undo();
                    Refresh();
                }
            }
        }
        
        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
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
            PlugInit(exportedTypes.ToArray());
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
                _polygon.Draw(e.Graphics, Radius, Lines, _rand);
            else
                _polygon.Draw(e.Graphics, Radius, Lines);
            /*
            test.Draw(e.Graphics, false, _polygon.VertexColor, _polygon.LineColor, _polygon.LineWidth, _polygon.VertexR);
            if (!flag)
            {
                (p_to, p_from) = (p_from, p_to);
                flag = true;
                ttmp(e.Graphics);
            }
            */
        }

        private void DynamicsButton_Click(object sender, EventArgs e) // динамика
        {
            _dynamicsTimer.Enabled = !_dynamicsTimer.Enabled;
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
        #endregion
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

        #region Mouse actions
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_polygon.DragAndDrop)
            {
                _polygon.DragAndDropCheck(e.Location);
                Refresh();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            _polygon.DragAndDropStop();
            Refresh();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _polygon.DragAndDropCheck(e.Location);
                if (!_polygon.DragAndDrop)
                    _polygon.Add((Vertex)Activator.CreateInstance(_newFigureShape, e.X, e.Y));
            }
            else
                _polygon.RemoveRange(_polygon.CheckVertices(e.Location));

            Refresh();
        }
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
            int checkedIndex = -1;
            for (int i = 0; i < shapeToolStripMenuItem.DropDownItems.Count && checkedIndex == -1; ++i)
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
            _polygon.VertexColor = (ChooseColor(_polygon.VertexColor));
        }

        private void ColorStripMenuLines_Click(object sender, EventArgs e)
        {
            _polygon.LineColor = (ChooseColor(_polygon.LineColor));
        }

        private void ColorStripMenuBackground_Click(object sender, EventArgs e) // Todo: add undo/redo
        {
            BackColor = ChooseColor(BackColor);
            Refresh();
        }
        #endregion
        #region Resize
        private void ResizeStripMenuVertices_Click(object sender, EventArgs e)
        {
            //dir = len - abs(len);

            TrackBar ResizeBar = new TrackBar
            {
                Minimum = 5,
                Maximum = 150,
                Value = _polygon.VertexR,
                Orientation = Orientation.Horizontal,
                TickStyle = TickStyle.None,
                Location = new Point(0, 0),
                Width = 400,

            };
            ResizeBar.Scroll += (_, __) => { Radius = ResizeBar.Value; Refresh(); };
            ResizeBar.MouseUp += (_, __) => { _polygon.VertexR = ResizeBar.Value; };

            Form SizeForm = new Form
            {
                ClientSize = new Size(ResizeBar.Width, ResizeBar.Height),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                MinimizeBox = false,
                MaximizeBox = false,
                Visible = true,
                KeyPreview = true
            };

            SizeForm.Controls.Add(ResizeBar);
            SizeForm.KeyDown += Form1_KeyDown;
            SizeForm.Deactivate += (_, __) => { SizeForm.Close(); SizeForm.Dispose(); };
            SizeForm.Disposed += (_, __) => { Radius = null; _polygon.VertexR = ResizeBar.Value; };
        }

        private void ResizeStripMenuLines_Click(object sender, EventArgs e)
        {
            TrackBar ResizeBar = new TrackBar
            {
                Minimum = 1,
                Maximum = 60,
                Value = _polygon.LineWidth,
                Orientation = Orientation.Horizontal,
                TickStyle = TickStyle.None,
                Location = new Point(0, 0),
                Width = 400,

            };
            ResizeBar.Scroll += (_, __) => { Lines = ResizeBar.Value; Refresh(); };
            ResizeBar.MouseUp += (_, __) => { _polygon.LineWidth = ResizeBar.Value; };

            Form SizeForm = new Form
            {
                ClientSize = new Size(ResizeBar.Width, ResizeBar.Height),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                MinimizeBox = false,
                MaximizeBox = false,
                Visible = true,
                KeyPreview = true
            };

            SizeForm.Controls.Add(ResizeBar);
            SizeForm.KeyDown += Form1_KeyDown;
            SizeForm.Deactivate += (_, __) => { SizeForm.Close(); SizeForm.Dispose(); };
            SizeForm.Disposed += (_, __) => { Lines = null; _polygon.LineWidth = ResizeBar.Value; };
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
