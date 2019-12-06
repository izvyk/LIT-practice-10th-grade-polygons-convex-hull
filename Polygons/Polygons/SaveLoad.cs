using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace Polygons
{
    public static class SaveLoadManager
    {
        private static readonly BinaryFormatter _binaryFormatter;
        private static readonly SaveFileDialog _saveFileDialog;
        private static readonly OpenFileDialog _openFileDialog;

        static SaveLoadManager()
        {
            _binaryFormatter = new BinaryFormatter();

            _saveFileDialog = new SaveFileDialog
            {
                AddExtension = true,
                CheckPathExists = true,
                DefaultExt = "plg"
            };

            _openFileDialog = new OpenFileDialog
            {
                Filter = "plg files (*.plg)|*.plg|All files (*.*)|*.*"
            };
        }

        public static void Save(IMemento obj, string directory = null)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            if (directory != null) _saveFileDialog.InitialDirectory = directory;

            _saveFileDialog.FileName =
                DateTime.Now.Year + "-" +
                DateTime.Now.Month + "-" +
                DateTime.Now.Day + "_" +
                DateTime.Now.Hour + "-" +
                DateTime.Now.Minute + "_data";

            if (_saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Stream SaveFileStream = File.Create(_saveFileDialog.FileName);

                _binaryFormatter.Serialize(SaveFileStream, obj);

                SaveFileStream.Close();
            }
        }

        public static IMemento Load(string directory = null)
        {
            if (directory != null) _openFileDialog.InitialDirectory = directory;

            IMemento memento = null;
            if (_openFileDialog.ShowDialog() == DialogResult.OK && File.Exists(_openFileDialog.FileName))
            {
                Stream openFileStream = File.OpenRead(_openFileDialog.FileName);
                try
                {
                    memento = (IMemento)_binaryFormatter.Deserialize(openFileStream);
                }
                catch
                {
                    MessageBox.Show("Unable to find a plugin!");
                    memento = null;
                }
                openFileStream.Close();
            }
            return memento;
        }
    }

    interface IOriginator
    {
        void SetMemento(in IMemento data);
        IMemento CreateMemento();
    }

    public interface IMemento
    {
        ExpandoObject GetState();
    }

    [Serializable]
    public class Form1Memento : IMemento
    {

        private readonly ConvexPolygon _polygon;
        private readonly Type _type;
        private readonly Color _backColor;
        private readonly IEnumerable<Type> _figureTypes;
        private readonly int _checkedIndex;

        public Form1Memento(in ConvexPolygon polygon, in Type type, in Color backColor, in IEnumerable<Type> figureTypes, in int checkedIndex)
        {
            _polygon = new ConvexPolygon(polygon ?? throw new ArgumentNullException("polygon")); // ??? Надо?
            _type = type ?? throw new ArgumentNullException("type");
            _figureTypes = figureTypes ?? throw new ArgumentNullException("figureTypes");
            _backColor = backColor;
            _checkedIndex = checkedIndex;
        }

        public ExpandoObject GetState()
        {
            dynamic state = new ExpandoObject();

            state.Polygon = new ConvexPolygon(_polygon);
            state.Type = _type;
            state.BackColor = _backColor;
            state.FigureTypes = _figureTypes;
            state.CheckedIndex = _checkedIndex;

            return state;
        }
    }
}
