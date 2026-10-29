using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace BerTaDEV.Toolkit
{
    /// <summary>Browser-style back/forward through what you selected (Alt+Left / Alt+Right).</summary>
    [InitializeOnLoad]
    internal static class SelectionHistory
    {
        const int MaxEntries = 64;
        const string BackPath = "Tools/BerTaDEV/Selection/Back &LEFT";
        const string ForwardPath = "Tools/BerTaDEV/Selection/Forward &RIGHT";

        static readonly List<Object[]> history = new List<Object[]>();
        static int index = -1;

        static SelectionHistory()
        {
            Selection.selectionChanged += Record;
        }

        static void Record()
        {
            var current = Selection.objects;
            if (current.Length == 0) return;

            // Navigating back/forward changes the selection too; that must not create a new entry.
            if (index >= 0 && SameObjects(history[index], current)) return;

            history.RemoveRange(index + 1, history.Count - index - 1);
            history.Add(current);
            if (history.Count > MaxEntries) history.RemoveAt(0);
            index = history.Count - 1;
        }

        [MenuItem(BackPath, priority = 300)]
        static void Back() => Navigate(-1);

        [MenuItem(ForwardPath, priority = 301)]
        static void Forward() => Navigate(1);

        [MenuItem(BackPath, true)]
        static bool CanGoBack() => FindAlive(-1) >= 0;

        [MenuItem(ForwardPath, true)]
        static bool CanGoForward() => FindAlive(1) >= 0;

        static void Navigate(int direction)
        {
            int target = FindAlive(direction);
            if (target < 0) return;
            index = target;
            Selection.objects = history[target].Where(o => o != null).ToArray();
        }

        /// <summary>Next entry in the given direction that still has at least one living object.</summary>
        static int FindAlive(int direction)
        {
            for (int i = index + direction; i >= 0 && i < history.Count; i += direction)
                if (history[i].Any(o => o != null)) return i;
            return -1;
        }

        static bool SameObjects(Object[] entry, Object[] current)
        {
            var alive = entry.Where(o => o != null).ToArray();
            return alive.Length == current.Length && !alive.Except(current).Any();
        }
    }
}
