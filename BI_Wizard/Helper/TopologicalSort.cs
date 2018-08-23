using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BI_Wizard.Helper
{
    class TopologicalSort
    {
        //http://www.codeproject.com/Articles/869059/Topological-sorting-in-Csharp

        public static IList<ICollection<T>> Group<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> getDependencies, IEqualityComparer<T> comparer = null, bool ignoreCycles = true)
        {
            var sorted = new List<ICollection<T>>();
            var visited = new Dictionary<T, int>(comparer);

            foreach (var item in source)
            {
                Visit(item, getDependencies, sorted, visited, ignoreCycles);
            }

            return sorted;
        }

        public static int Visit<T>(T item, Func<T, IEnumerable<T>> getDependencies, List<ICollection<T>> sorted, Dictionary<T, int> visited, bool ignoreCycles)
        {
            const int inProcess = -1;
            int level;
            var alreadyVisited = visited.TryGetValue(item, out level);

            if (alreadyVisited)
            {
                if (level == inProcess && ignoreCycles)
                {
                    throw new ArgumentException("Cyclic dependency found.");
                }
            }
            else
            {
                visited[item] = (level = inProcess);

                var dependencies = getDependencies(item);
                if (dependencies != null)
                {
                    foreach (var dependency in dependencies)
                    {
                        var depLevel = Visit(dependency, getDependencies, sorted, visited, ignoreCycles);
                        level = Math.Max(level, depLevel);
                    }
                }

                visited[item] = ++level;
                while (sorted.Count <= level)
                {
                    sorted.Add(new Collection<T>());
                }
                sorted[level].Add(item);
            }
            return level;
        }
    }
}
