using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Reflection;

namespace Polygons
{
    static class PluginManager
    {
        private static IEnumerable<Type> Load(string plugPath)
        {
            if (plugPath is null) throw new ArgumentNullException(nameof(plugPath));
            if (!File.Exists(plugPath)) throw new ArgumentException($"File not found: \"{ plugPath }\"");

            return Assembly.LoadFrom(plugPath).GetExportedTypes();
        }

        public static IEnumerable<Type> LoadFromDirectory(string directory = null, string fileMask = null, SearchOption searchOptions = SearchOption.AllDirectories)
        {
            if (string.IsNullOrEmpty(directory)) directory = Environment.CurrentDirectory;
            if (string.IsNullOrEmpty(fileMask)) fileMask = "*";

            var exportedTypes = new List<Type>();

            foreach (var path in Directory.GetFiles(directory, fileMask, searchOptions))
                exportedTypes.AddRange(Load(path));
            return exportedTypes;
        }

        public static IEnumerable<Type> LoadFile(string fileNameFull)
        {
            if (fileNameFull is null) throw new ArgumentNullException(nameof(fileNameFull));
            if (fileNameFull.Length == 0) throw new ArgumentException();

            return Load(fileNameFull);
        }
    }
}
