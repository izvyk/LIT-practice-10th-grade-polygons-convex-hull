using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;

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

        private static List<Type> Load(params string[] plugPaths)
        {

            List<Type> types = new List<Type>();

            foreach (string path in plugPaths)
            {
                Type[] tmp = Assembly.LoadFrom(path).GetExportedTypes();
                foreach (var i in tmp)
                {
                    if (_exportedTypes.Contains(i))
                        throw new Exception(String.Format("Error appeared while addind plugin: plugin named \"{0}\" already exists!", i.Name));
                }

                types.AddRange(tmp);
            }

            _exportedTypes.AddRange(types);
            return types;
        }

        public static List<Type> LoadFromDirectory(string directory = null, string fileMask = null, SearchOption searchOptions = SearchOption.AllDirectories)
        {
            if (directory == null || directory.Length == 0) directory = Environment.CurrentDirectory;
            if (fileMask == null || fileMask.Length == 0) fileMask = "*";

            string[] PlugPaths = Directory.GetFiles(directory, fileMask, searchOptions);
            try
            {
                return Load(PlugPaths);
            }
            catch (Exception e)
            {
                ExceptionDispatchInfo di = ExceptionDispatchInfo.Capture(e);
                di.Throw();
                return null;
            }

            // Todo: this is unriched code
        }

        public static List<Type> LoadFile(string fileNameFull)
        {
            if (fileNameFull == null) throw new ArgumentNullException("fileNameFull");
            if (fileNameFull.Length == 0) throw new ArgumentException();

            try
            {
                return Load(fileNameFull);
            }
            catch (Exception e)
            {
                ExceptionDispatchInfo di = ExceptionDispatchInfo.Capture(e);
                di.Throw();
                return null;
            }

            // Todo: this is unriched code
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

        public static void MemorisePlugin(params Type[] plugins)
        {
            if (plugins == null) throw new ArgumentNullException("plugins");
            _exportedTypes.AddRange(plugins);
        }
    }
}
