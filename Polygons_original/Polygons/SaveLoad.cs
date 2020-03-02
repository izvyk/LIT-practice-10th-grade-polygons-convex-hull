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
        private static readonly BinaryFormatter BinaryFormatter;
        private static readonly SaveFileDialog SaveFileDialog;
        private static readonly OpenFileDialog OpenFileDialog;

        /* Lazy loader
        private static readonly Lazy<BinaryFormatter> _binaryFormatter;
        private static readonly Lazy<SaveFileDialog> _saveFileDialog;
        private static readonly Lazy<OpenFileDialog> _openFileDialog;

        static SaveLoadManager()
        {
            _binaryFormatter = new Lazy<BinaryFormatter>(() => new BinaryFormatter());
            _saveFileDialog = new Lazy<SaveFileDialog>(() => new SaveFileDialog
                {
                    AddExtension = true,
                    CheckPathExists = true,
                    DefaultExt = "plg"
                }
            );
            _openFileDialog = new Lazy<OpenFileDialog>(() => new OpenFileDialog
            {
                Filter = "plg files (*.plg)|*.plg|All files (*.*)|*.*"
            });
        }
        */

        static SaveLoadManager()
        {
            BinaryFormatter = new BinaryFormatter();

            SaveFileDialog = new SaveFileDialog
            {
                AddExtension = true,
                CheckPathExists = true,
                DefaultExt = "plg"
            };

            OpenFileDialog = new OpenFileDialog
            {
                Filter = @"plg files (*.plg)|*.plg|All files (*.*)|*.*"
            };
        }

        public static void Save(IMemento obj, string directory = null)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (directory != null) SaveFileDialog.InitialDirectory = directory;

            SaveFileDialog.FileName =
                DateTime.Now.Year + "-" +
                DateTime.Now.Month + "-" +
                DateTime.Now.Day + "_" +
                DateTime.Now.Hour + "-" +
                DateTime.Now.Minute + "_data";

            if (SaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Stream saveFileStream = File.Create(SaveFileDialog.FileName);

                BinaryFormatter.Serialize(saveFileStream, obj);

                saveFileStream.Close();
            }
        }

        public static IMemento Load(string directory = null)
        {
            if (directory != null) OpenFileDialog.InitialDirectory = directory;

            IMemento memento = null;
            if (OpenFileDialog.ShowDialog() == DialogResult.OK && File.Exists(OpenFileDialog.FileName))
            {
                Stream openFileStream = File.OpenRead(OpenFileDialog.FileName);
                try
                {
                    memento = (IMemento)BinaryFormatter.Deserialize(openFileStream);
                }
                catch
                {
                    MessageBox.Show(@"Unable to find a plugin!");
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
            _polygon = new ConvexPolygon(polygon ?? throw new ArgumentNullException(nameof(polygon))); // ??? Надо?
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _figureTypes = figureTypes ?? throw new ArgumentNullException(nameof(figureTypes));
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
