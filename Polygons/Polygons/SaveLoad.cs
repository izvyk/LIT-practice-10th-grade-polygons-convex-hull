using System;
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

        static SaveLoadManager()
        {
            BinaryFormatter = new BinaryFormatter();

            SaveFileDialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "plg",
                CheckPathExists = true,
                OverwritePrompt = true
            };

            OpenFileDialog = new OpenFileDialog
            {
                Filter = @"plg files (*.plg)|*.plg|All files (*.*)|*.*",
                CheckPathExists = true,
                CheckFileExists = true,
                Multiselect = false,
                Title = @"Save as..."
            };
        }

        public static void SetSaveInitialDirectory(in string directory)
        {
            if (directory?.Length > 0)
                SaveFileDialog.InitialDirectory = directory;
        }

        public static void SetOpenInitialDirectory(in string directory)
        {
            if (directory?.Length > 0)
                OpenFileDialog.InitialDirectory = directory;
        }

        public static void Save(params object[] data)
        {
            if (data is null || data.Length == 0) throw new ArgumentNullException(nameof(data));

            SaveFileDialog.FileName =
                DateTime.Now.Year + "-" +
                DateTime.Now.Month + "-" +
                DateTime.Now.Day + "_" +
                DateTime.Now.Hour + "-" +
                DateTime.Now.Minute + "_data";

            if (SaveFileDialog.ShowDialog() != DialogResult.OK) return;

            using Stream SaveFileStream = File.Create(SaveFileDialog.FileName);
            BinaryFormatter.Serialize(SaveFileStream, data);
        }

        public static object[] Load()
        {
            if (OpenFileDialog.ShowDialog() != DialogResult.OK || !File.Exists(OpenFileDialog.FileName)) return null;
            using Stream openFileStream = File.OpenRead(OpenFileDialog.FileName);
            return BinaryFormatter.Deserialize(openFileStream) as object[] ?? throw new DllNotFoundException();
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
