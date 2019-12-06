using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Polygons
{
    static class PluginManager
    {
        private static readonly List<Type> _exportedTypes;

        public static IReadOnlyCollection<Type> GetTypes() => _exportedTypes.AsReadOnly();

        static PluginManager()
        {
            _exportedTypes = new List<Type>();
        }

        public static List<Type> Load(string directory = null, string fileMask = null, SearchOption searchOptions = SearchOption.AllDirectories)
        {
            if (directory == null || directory.Length == 0) directory = Environment.CurrentDirectory;
            if (fileMask == null || fileMask.Length == 0) fileMask = "*";

            string[] PlugPaths = Directory.GetFiles(directory, fileMask, searchOptions);

            foreach (string path in PlugPaths)
                _exportedTypes.AddRange(Assembly.LoadFrom(path).GetExportedTypes());

            return _exportedTypes;
        }

        public static Type GetPlugin(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (name.Length == 0) throw new ArgumentException();

            return _exportedTypes.Find(delegate (Type t)
            {
                return t.Name.Contains(name);
            });
        }
    }
}
