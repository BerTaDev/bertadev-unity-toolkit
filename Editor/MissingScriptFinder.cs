using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BerTaDEV.Toolkit
{
    /// <summary>Finds and removes "Missing (Mono Script)" components in open scenes and prefab assets.</summary>
    internal sealed class MissingScriptFinder : EditorWindow
    {
        [Serializable]
        class Entry
        {
            public GameObject target;
            public string prefabPath; // empty for objects that live in a scene
            public string label;
            public int count;

            public bool IsPrefabAsset => !string.IsNullOrEmpty(prefabPath);
        }

        [SerializeField] List<Entry> entries = new List<Entry>();
        [SerializeField] bool includePrefabs = true;
        [SerializeField] bool hasScanned;
        Vector2 scroll;

        [MenuItem("Tools/BerTaDEV/Missing Script Finder", priority = 101)]
        static void Open()
        {
            var window = GetWindow<MissingScriptFinder>();
            window.titleContent = new GUIContent("Missing Scripts", EditorGUIUtility.IconContent("console.warnicon.sml").image);
            window.minSize = new Vector2(320f, 200f);
        }

        void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                // Work that shows progress bars or saves assets runs after OnGUI to keep the layout intact.
                if (GUILayout.Button("Scan", EditorStyles.toolbarButton, GUILayout.Width(50f)))
                    EditorApplication.delayCall += Scan;
                includePrefabs = GUILayout.Toggle(includePrefabs, "Include Prefabs", EditorStyles.toolbarButton);
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(entries.Count == 0))
                {
                    if (GUILayout.Button("Remove All", EditorStyles.toolbarButton, GUILayout.Width(80f)))
                        EditorApplication.delayCall += RemoveAll;
                }
            }

            if (!hasScanned)
            {
                EditorGUILayout.HelpBox("Scan the open scenes (and optionally every prefab under Assets/) for components whose script is missing.", MessageType.Info);
                return;
            }
            if (entries.Count == 0)
            {
                EditorGUILayout.HelpBox("No missing scripts found.", MessageType.Info);
                return;
            }

            int total = 0;
            foreach (var entry in entries) total += entry.count;
            EditorGUILayout.HelpBox($"{total} missing script(s) on {entries.Count} GameObject(s). Click a row to select it.", MessageType.Warning);

            var sceneIcon = EditorGUIUtility.IconContent("GameObject Icon").image;
            var prefabIcon = EditorGUIUtility.IconContent("Prefab Icon").image;
            EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));

            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var entry in entries)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var content = new GUIContent(entry.label, entry.IsPrefabAsset ? prefabIcon : sceneIcon, entry.IsPrefabAsset ? entry.prefabPath : "Scene object");
                    if (GUILayout.Button(content, EditorStyles.label, GUILayout.MinWidth(100f)) && entry.target != null)
                    {
                        Selection.activeObject = entry.target;
                        EditorGUIUtility.PingObject(entry.target);
                    }
                    GUILayout.Label("x" + entry.count, EditorStyles.miniLabel, GUILayout.Width(28f));
                    if (GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(60f)))
                    {
                        var captured = entry;
                        EditorApplication.delayCall += () => RemoveOne(captured);
                    }
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUIUtility.SetIconSize(Vector2.zero);
        }

        void Scan()
        {
            entries.Clear();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                    Collect(root, null, scene.name);
            }

            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null) Collect(stage.prefabContentsRoot, null, "Prefab Mode");

            if (includePrefabs)
            {
                var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
                try
                {
                    for (int i = 0; i < guids.Length; i++)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                        if (EditorUtility.DisplayCancelableProgressBar("Missing Script Finder", path, (float)i / guids.Length)) break;
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (prefab != null) Collect(prefab, path, Path.GetFileNameWithoutExtension(path));
                    }
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }

            hasScanned = true;
            Repaint();
        }

        void Collect(GameObject root, string prefabPath, string context)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                if (count == 0) continue;
                entries.Add(new Entry
                {
                    target = t.gameObject,
                    prefabPath = prefabPath,
                    label = context + " > " + HierarchyPath(t),
                    count = count,
                });
            }
        }

        void RemoveOne(Entry entry)
        {
            if (entry.target == null) return;
            if (entry.IsPrefabAsset)
            {
                if (!EditorUtility.DisplayDialog("Remove missing scripts",
                        $"Remove every missing script in '{entry.prefabPath}' and save the prefab?\nThis can't be undone.", "Remove", "Cancel"))
                    return;
                RemoveFromPrefabAsset(entry.prefabPath);
            }
            else
            {
                RemoveFromSceneObject(entry.target);
            }
            Recount();
        }

        void RemoveAll()
        {
            int total = 0;
            foreach (var entry in entries) total += entry.count;
            if (!EditorUtility.DisplayDialog("Remove missing scripts",
                    $"Remove {total} missing script component(s)?\nScene changes can be undone, prefab assets are saved directly.", "Remove", "Cancel"))
                return;

            // Prefabs first: scene instances can only lose a missing script once their source prefab has.
            var prefabPaths = new HashSet<string>();
            foreach (var entry in entries)
                if (entry.IsPrefabAsset) prefabPaths.Add(entry.prefabPath);
            foreach (var path in prefabPaths) RemoveFromPrefabAsset(path);

            foreach (var entry in entries)
            {
                if (entry.IsPrefabAsset || entry.target == null) continue;
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(entry.target) > 0)
                    RemoveFromSceneObject(entry.target);
            }
            Recount();
        }

        static void RemoveFromSceneObject(GameObject go)
        {
            Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts");
            if (TryRemove(go) > 0) EditorSceneManager.MarkSceneDirty(go.scene);
        }

        static void RemoveFromPrefabAsset(string path)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                int removed = 0;
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    removed += TryRemove(t.gameObject);
                if (removed > 0) PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static int TryRemove(GameObject go)
        {
            try
            {
                int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                if (removed == 0 && PrefabUtility.IsPartOfPrefabInstance(go))
                    Debug.LogWarning($"[BerTaDEV] '{go.name}' inherits its missing script from a prefab. Fix the source prefab instead.", go);
                return removed;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BerTaDEV] Could not remove missing scripts from '{go.name}': {ex.Message}", go);
                return 0;
            }
        }

        void Recount()
        {
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                var entry = entries[i];
                entry.count = entry.target != null ? GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(entry.target) : 0;
                if (entry.count == 0) entries.RemoveAt(i);
            }
            Repaint();
        }

        static string HierarchyPath(Transform t)
        {
            string path = t.name;
            for (var parent = t.parent; parent != null; parent = parent.parent)
                path = parent.name + "/" + path;
            return path;
        }
    }
}
