using System;
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

        public static void Save(object data, string directory = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (directory != null) _saveFileDialog.InitialDirectory = directory;

            _saveFileDialog.FileName =
                DateTime.Now.Year + "-" +
                DateTime.Now.Month + "-" +
                DateTime.Now.Day + "_" +
                DateTime.Now.Hour + "-" +
                DateTime.Now.Minute + "_data";

            if (_saveFileDialog.ShowDialog() != DialogResult.OK) return;

            using (Stream SaveFileStream = File.Create(_saveFileDialog.FileName))
                _binaryFormatter.Serialize(SaveFileStream, data);
        }

        public static object Load(string directory = null)
        {
            if (directory != null) _openFileDialog.InitialDirectory = directory;

            object data = null;
            if (_openFileDialog.ShowDialog() == DialogResult.OK && File.Exists(_openFileDialog.FileName))
            {
                using (Stream openFileStream = File.OpenRead(_openFileDialog.FileName))
                {
                    try
                    {
                        data = (IMemento)_binaryFormatter.Deserialize(openFileStream);
                    }
                    catch
                    {
                        MessageBox.Show("Unable to find a plugin!");
                        data = null;
                    }
                }
            }
            return data;
        }

    }
    
    public interface IOriginator
    {
        IMemento CreateMemento();
        void SetMemento(in IMemento memento);
    }

    public interface IMemento
    {
        ExpandoObject GetState();
    }

}
