using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Polygons
{
    static class PluginManager
    {
        private static readonly List<Type> ExportedTypes;

        public static IReadOnlyCollection<Type> GetTypes() => ExportedTypes.AsReadOnly();

        static PluginManager()
        {
            ExportedTypes = new List<Type>();
        }

        private static List<Type> Load(params string[] plugPaths)
        {

            var types = new List<Type>();

            foreach (var path in plugPaths)
            {
                var tmp = Assembly.LoadFrom(path).GetExportedTypes();
                foreach (var i in tmp)
                {
                    if (ExportedTypes.Contains(i))
                        throw new Exception(
                            $"Error appeared while adding plugin: plugin named \"{i.Name}\" already exists!");
                }

                types.AddRange(tmp);
            }

            ExportedTypes.AddRange(types);
            return types;
        }

        public static List<Type> LoadFromDirectory(string directory = null, string fileMask = null, SearchOption searchOptions = SearchOption.AllDirectories)
        {
            if (string.IsNullOrEmpty(directory)) directory = Environment.CurrentDirectory;
            if (string.IsNullOrEmpty(fileMask)) fileMask = "*";

            var plugPaths = Directory.GetFiles(directory, fileMask, searchOptions);
            try
            {
                return Load(plugPaths);
            }
            catch (Exception e)
            {
                var di = ExceptionDispatchInfo.Capture(e);
                di.Throw();
                return null;
            }

            // Todo: this is unreached code
        }

        public static List<Type> LoadFile(string fileNameFull)
        {
            if (fileNameFull == null) throw new ArgumentNullException(nameof(fileNameFull));
            if (fileNameFull.Length == 0) throw new ArgumentException();

            try
            {
                return Load(fileNameFull);
            }
            catch (Exception e)
            {
                var di = ExceptionDispatchInfo.Capture(e);
                di.Throw();
                return null;
            }

            // Todo: this is unreached code
        }

/*
        public static Type GetPlugin(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (name.Length == 0) throw new ArgumentException();

            return ExportedTypes.Find(delegate (Type t)
            {
                return t.Name.Contains(name);
            });
        }
*/

        public static void MemorisePlugin(params Type[] plugins)
        {
            if (plugins == null) throw new ArgumentNullException(nameof(plugins));
            ExportedTypes.AddRange(plugins);
        }
    }

    /*
    interface ISmooth
    {
        Point this[int index] { get; }
        bool Moving { get; set; }

        #region List-like actions
        void Add(in Vertex NewItem, in Point to);

        void AddRange(in ICollection<Vertex> NewItems, in ICollection<Point> to);

        void RemoveAt(in int index);

        void Remove(Vertex Item);

        void Clear();
        #endregion
    }
    
    static class DynamicsManager
    {
        private struct Item
        {
            public Item(in Vertex what, in Point from, in Point to, in bool flag)
            {
                this.what = what;
                this.from = from;
                this.to = to;
                this.flag = flag;
            }

            public Vertex what;
            public Point from;
            public Point to;
            public bool flag;
        }

        //private static readonly List<Point> _what, _from, _to;
        private static readonly List<Item> objects;
        private static Random _random;
        private static Timer _timer;
        public static IEnumerable<Point> From => from item in objects
                                                         select item.@from;
        public static IEnumerable<Point> To => from item in objects
                                               select item.to;
        public static int Count => objects.Count;
        private static bool _flag = false;
        public static bool Flag { get => _flag;
            set
            {
                if (_flag != value)
                {
                    if (_flag)
                    {
                        _timer.Stop();
                    }
                    else
                    {
                        _timer.Start();
                    }
                }

                _flag = value;
            }
        }

        static DynamicsManager()
        {
            objects = new List<Item>();
            //_from = new List<Point>();
            //_to = new List<Point>();

            _random = new Random();
            _timer = new Timer() { Interval = 15 };

            _timer.Tick += (o, ev) =>
            {
                //g.Clear(BackColor);
                //test.Draw(g, false, _polygon.VertexColor, _polygon.LineColor, _polygon.LineWidth, _polygon.VertexR);
                //Refresh();
                int dx = (int)Math.Ceiling((decimal)((p_to.X - test.X) / 30));
                int dy = (int)Math.Ceiling((decimal)((p_to.Y - test.Y) / 30));
                test.X += dx;
                test.Y += dy;

                if (Math.Abs(dx) < 1 && Math.Abs(dy) < 1)
                {
                    _timer.Stop();
                    flag = false;
                }
            };
        }

        public static void Init(Delegate refresh)
        {
            _timer.Tick += (_, __) =>
            {
                for (int i = 0; i < Count; ++i)
                {
                    int dx = (int)Math.Ceiling((decimal)((p_to.X - test.X) / 30));
                    int dy = (int)Math.Ceiling((decimal)((p_to.Y - test.Y) / 30));
                    _[i].X += dx;
                    test.Y += dy;

                    if (Math.Abs(dx) < 1 && Math.Abs(dy) < 1)
                    {
                        RemoveAt(i);
                    }
                }

                refresh.DynamicInvoke();
            };
        }

        private static void AnimationStart()
        {

        }

        #region List-like actions
        public static void Add(in Vertex NewItem, in Point to)
        {
            //_from.Add(NewItem ?? throw new ArgumentNullException("NewItem"));
            //_what.Add(NewItem);
            //_to.Add(to);
            if (NewItem == null) throw new ArgumentNullException("NewItem"));
            objects.Add(new Item(NewItem, NewItem, to, false));
        }

        public static void AddRange(in ICollection<Vertex> NewItems, in ICollection<Point> to)
        {
            if (NewItems == null) throw new ArgumentNullException("NewItems");
            if (to == null) throw new ArgumentNullException("to");
            if (NewItems.Count != to.Count) throw new ArgumentException();

            //_from.AddRange(NewItems.Select(i => (Point)i));
            //_to.AddRange(to);
            objects.AddRange(NewItems.Select(i => new Item(i, i, new Point(0, 0), false)));

            objects.AddRange(from item in NewItems.Count
                             )
        }

        public static void RemoveAt(in int index)
        {
            if (index < 0 || index >= Count - 1) throw new IndexOutOfRangeException("index");
            _from.RemoveAt(index);
            _to.RemoveAt(index);
        }

        public static void Remove(Vertex Item)
        {
            if (Item == null) throw new ArgumentNullException("Item");

            int index = _from.FindIndex(i => i == Item);
            if (index < 0) throw new InvalidOperationException();
            _from.RemoveAt(index);
            _to.RemoveAt(index);
        }

        public static void Clear()
        {
            _from.Clear();
            _to.Clear();
        }
        #endregion
    }
    */
}
