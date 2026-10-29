using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BerTaDEV.Toolkit
{
    /// <summary>Lines the selected objects up on an axis, or spaces them out evenly.</summary>
    internal sealed class AlignDistribute : EditorWindow
    {
        internal enum Anchor { Min, Center, Max }
        enum RelativeTo { Selection, ActiveObject }

        static readonly string[] AxisNames = { "X", "Y", "Z" };
        static readonly Color[] AxisColors = { new Color(0.93f, 0.38f, 0.35f), new Color(0.6f, 0.85f, 0.35f), new Color(0.38f, 0.6f, 0.95f) };

        [SerializeField] RelativeTo relativeTo;
        [SerializeField] bool useBounds = true;

        [MenuItem("Tools/BerTaDEV/Align And Distribute", priority = 105)]
        static void Open()
        {
            var window = GetWindow<AlignDistribute>();
            window.titleContent = new GUIContent("Align & Distribute");
            window.minSize = new Vector2(300f, 190f);
        }

        void OnSelectionChange() => Repaint();

        void OnGUI()
        {
            EditorGUILayout.Space(4f);
            relativeTo = (RelativeTo)EditorGUILayout.EnumPopup(new GUIContent("Relative To", "Line up with the edges of the whole selection, or with the active (last clicked) object."), relativeTo);
            useBounds = EditorGUILayout.Toggle(new GUIContent("Use Renderer Bounds", "On: line up the visible edges. Off: line up pivots."), useBounds);
            EditorGUILayout.Space();

            var targets = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);
            var reference = relativeTo == RelativeTo.ActiveObject ? Selection.activeTransform : null;

            for (int axis = 0; axis < 3; axis++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var previousColor = GUI.contentColor;
                    GUI.contentColor = AxisColors[axis];
                    GUILayout.Label(AxisNames[axis], EditorStyles.boldLabel, GUILayout.Width(18f));
                    GUI.contentColor = previousColor;

                    using (new EditorGUI.DisabledScope(targets.Length < 2))
                    {
                        if (GUILayout.Button("Min")) Align(targets, axis, Anchor.Min, reference, useBounds);
                        if (GUILayout.Button("Center")) Align(targets, axis, Anchor.Center, reference, useBounds);
                        if (GUILayout.Button("Max")) Align(targets, axis, Anchor.Max, reference, useBounds);
                    }
                    using (new EditorGUI.DisabledScope(targets.Length < 3))
                    {
                        if (GUILayout.Button("Distribute")) Distribute(targets, axis, useBounds);
                    }
                }
            }

            EditorGUILayout.Space();
            string status = targets.Length switch
            {
                0 => "Select two or more objects.",
                1 => "Select at least one more object.",
                2 => "2 objects selected. Distribute needs three.",
                _ => $"{targets.Length} objects selected.",
            };
            EditorGUILayout.HelpBox(status, MessageType.None);
        }

        internal static void Align(Transform[] targets, int axis, Anchor anchor, Transform reference, bool useBounds)
        {
            if (targets.Length == 0) return;
            Physics.SyncTransforms();

            float goal;
            if (reference != null)
            {
                goal = Edge(reference, axis, anchor, useBounds);
            }
            else
            {
                // Edges of the whole selection; "center" is halfway between them.
                float min = targets.Min(t => Edge(t, axis, Anchor.Min, useBounds));
                float max = targets.Max(t => Edge(t, axis, Anchor.Max, useBounds));
                goal = anchor == Anchor.Min ? min : anchor == Anchor.Max ? max : (min + max) * 0.5f;
            }

            Undo.RecordObjects(targets, "Align " + AxisNames[axis]);
            foreach (var t in targets)
            {
                if (t == reference) continue;
                var position = t.position;
                position[axis] += goal - Edge(t, axis, anchor, useBounds);
                t.position = position;
            }
        }

        /// <summary>Keeps the two outermost objects where they are and spaces the others evenly between them.</summary>
        internal static void Distribute(Transform[] targets, int axis, bool useBounds)
        {
            if (targets.Length < 3) return;
            Physics.SyncTransforms();

            var sorted = targets.OrderBy(t => Edge(t, axis, Anchor.Center, useBounds)).ToArray();
            float first = Edge(sorted[0], axis, Anchor.Center, useBounds);
            float last = Edge(sorted[sorted.Length - 1], axis, Anchor.Center, useBounds);
            float step = (last - first) / (sorted.Length - 1);

            Undo.RecordObjects(sorted, "Distribute " + AxisNames[axis]);
            for (int i = 1; i < sorted.Length - 1; i++)
            {
                var position = sorted[i].position;
                position[axis] += first + step * i - Edge(sorted[i], axis, Anchor.Center, useBounds);
                sorted[i].position = position;
            }
        }

        static float Edge(Transform t, int axis, Anchor anchor, bool useBounds)
        {
            if (!useBounds) return t.position[axis];
            var bounds = BoundsUtility.GetWorldBounds(t);
            return anchor == Anchor.Min ? bounds.min[axis] : anchor == Anchor.Max ? bounds.max[axis] : bounds.center[axis];
        }
    }
}
