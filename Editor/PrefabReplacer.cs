using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BerTaDEV.Toolkit
{
    /// <summary>Swaps the selected GameObjects for instances of a prefab, keeping their place in the scene.</summary>
    internal sealed class PrefabReplacer : EditorWindow
    {
        [Serializable]
        internal struct Options
        {
            public bool keepRotation;
            public bool keepScale;
            public bool keepName;
        }

        [SerializeField] GameObject prefab;
        [SerializeField] Options options = new Options { keepRotation = true, keepScale = true };
        Vector2 scroll;

        [MenuItem("Tools/BerTaDEV/Prefab Replacer", priority = 103)]
        static void Open()
        {
            var window = GetWindow<PrefabReplacer>();
            window.titleContent = new GUIContent("Prefab Replacer", EditorGUIUtility.IconContent("Prefab Icon").image);
            window.minSize = new Vector2(300f, 260f);
        }

        void OnSelectionChange() => Repaint();

        void OnGUI()
        {
            EditorGUILayout.Space(4f);
            prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
            options.keepRotation = EditorGUILayout.Toggle("Keep Rotation", options.keepRotation);
            options.keepScale = EditorGUILayout.Toggle("Keep Scale", options.keepScale);
            options.keepName = EditorGUILayout.Toggle(new GUIContent("Keep Name", "Use the replaced object's name instead of the prefab's."), options.keepName);

            var targets = GetTargets();
            bool isPrefab = prefab != null && IsPrefabAsset(prefab);

            EditorGUILayout.Space();
            if (prefab == null)
                EditorGUILayout.HelpBox("Pick the prefab (or model) to place.", MessageType.Info);
            else if (!isPrefab)
                EditorGUILayout.HelpBox("That isn't a prefab or model asset.", MessageType.Warning);
            else if (targets.Count == 0)
                EditorGUILayout.HelpBox("Select the GameObjects to replace in the Hierarchy.", MessageType.Info);
            else
                EditorGUILayout.HelpBox($"Each selected object becomes an instance of '{prefab.name}' at the same spot in the Hierarchy. Their children are removed (Undo brings them back).", MessageType.Info);

            EditorGUILayout.LabelField($"Selected ({targets.Count})", EditorStyles.boldLabel);
            var icon = EditorGUIUtility.IconContent("GameObject Icon").image;
            EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var go in targets) EditorGUILayout.LabelField(new GUIContent(go.name, icon));
            EditorGUILayout.EndScrollView();
            EditorGUIUtility.SetIconSize(Vector2.zero);

            using (new EditorGUI.DisabledScope(!isPrefab || targets.Count == 0))
            {
                if (GUILayout.Button($"Replace {targets.Count} Object(s)", GUILayout.Height(28f)))
                {
                    var chosen = prefab;
                    var chosenOptions = options;
                    EditorApplication.delayCall += () => Selection.objects = Replace(chosen, GetTargets(), chosenOptions).ToArray();
                }
            }
            EditorGUILayout.Space(4f);
        }

        static List<GameObject> GetTargets()
        {
            var targets = new List<GameObject>();
            foreach (var go in Selection.GetFiltered<GameObject>(SelectionMode.TopLevel | SelectionMode.ExcludePrefab | SelectionMode.Editable))
                if (!EditorUtility.IsPersistent(go)) targets.Add(go);
            return targets;
        }

        static bool IsPrefabAsset(GameObject go)
        {
            if (!EditorUtility.IsPersistent(go)) return false;
            var type = PrefabUtility.GetPrefabAssetType(go);
            return type != PrefabAssetType.NotAPrefab && type != PrefabAssetType.MissingAsset;
        }

        internal static List<GameObject> Replace(GameObject prefab, IList<GameObject> targets, Options options)
        {
            var created = new List<GameObject>();
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();

            foreach (var target in targets)
            {
                if (target == null) continue;
                if (PrefabUtility.IsPartOfPrefabInstance(target) && !PrefabUtility.IsOutermostPrefabInstanceRoot(target))
                {
                    Debug.LogWarning($"[BerTaDEV] Skipped '{target.name}': it's inside a prefab instance. Open that prefab or unpack it first.", target);
                    continue;
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, target.scene);
                if (instance == null) continue;
                Undo.RegisterCreatedObjectUndo(instance, "Replace With Prefab");

                var source = target.transform;
                var t = instance.transform;
                t.SetParent(source.parent, false);
                t.SetSiblingIndex(source.GetSiblingIndex());
                t.localPosition = source.localPosition;
                if (options.keepRotation) t.localRotation = source.localRotation;
                if (options.keepScale) t.localScale = source.localScale;
                if (options.keepName) instance.name = target.name;
                instance.SetActive(target.activeSelf);

                created.Add(instance);
                Undo.DestroyObjectImmediate(target);
            }

            Undo.SetCurrentGroupName("Replace With Prefab");
            Undo.CollapseUndoOperations(group);
            return created;
        }
    }
}
