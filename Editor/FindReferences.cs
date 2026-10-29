using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace BerTaDEV.Toolkit
{
    /// <summary>Lists the assets and the open-scene objects that use a given asset.</summary>
    internal sealed class FindReferences : EditorWindow
    {
        [Serializable]
        class SceneHit
        {
            public GameObject gameObject;
            public string label;
        }

        // Reverse dependency index (asset path -> assets that use it directly), shared by every window
        // and thrown away whenever the project changes.
        static Dictionary<string, List<string>> index;
        static int indexedAssetCount;

        [SerializeField] Object target;
        [SerializeField] List<string> assetHits = new List<string>();
        [SerializeField] List<SceneHit> sceneHits = new List<SceneHit>();
        [SerializeField] bool searched;
        Vector2 scroll;

        [InitializeOnLoadMethod]
        static void InvalidateOnProjectChange() => EditorApplication.projectChanged += () => index = null;

        [MenuItem("Tools/BerTaDEV/Find References", priority = 104)]
        static void OpenEmpty() => Open(null);

        [MenuItem("Assets/BerTaDEV/Find References", priority = 30)]
        static void OpenForSelection() => Open(Selection.activeObject);

        [MenuItem("Assets/BerTaDEV/Find References", true)]
        static bool CanOpenForSelection() => Selection.activeObject != null && EditorUtility.IsPersistent(Selection.activeObject);

        static void Open(Object asset)
        {
            var window = GetWindow<FindReferences>();
            window.titleContent = new GUIContent("Find References", EditorGUIUtility.IconContent("Search Icon").image);
            window.minSize = new Vector2(320f, 240f);
            if (asset == null) return;
            window.target = asset;
            EditorApplication.delayCall += window.Search;
        }

        void OnGUI()
        {
            EditorGUILayout.Space(4f);
            EditorGUI.BeginChangeCheck();
            target = EditorGUILayout.ObjectField("Asset", target, typeof(Object), false);
            if (EditorGUI.EndChangeCheck()) ClearResults();

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(target == null))
                {
                    if (GUILayout.Button("Find References", GUILayout.Height(24f)))
                        EditorApplication.delayCall += Search;
                }
                if (GUILayout.Button("Use Selection", GUILayout.Width(100f), GUILayout.Height(24f))
                    && Selection.activeObject != null && EditorUtility.IsPersistent(Selection.activeObject))
                {
                    target = Selection.activeObject;
                    EditorApplication.delayCall += Search;
                }
            }

            if (!searched)
            {
                EditorGUILayout.HelpBox("Pick an asset, or right-click one in the Project window > BerTaDEV > Find References.", MessageType.Info);
            }
            else
            {
                EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
                scroll = EditorGUILayout.BeginScrollView(scroll);

                EditorGUILayout.LabelField($"Used by {assetHits.Count} asset(s)", EditorStyles.boldLabel);
                foreach (string path in assetHits)
                {
                    if (GUILayout.Button(new GUIContent(path, AssetDatabase.GetCachedIcon(path)), EditorStyles.label))
                        Ping(AssetDatabase.LoadMainAssetAtPath(path));
                }
                if (assetHits.Count == 0) EditorGUILayout.LabelField("Nothing under Assets/ uses it.", EditorStyles.miniLabel);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"In open scenes ({sceneHits.Count})", EditorStyles.boldLabel);
                var icon = EditorGUIUtility.IconContent("GameObject Icon").image;
                foreach (var hit in sceneHits)
                {
                    if (GUILayout.Button(new GUIContent(hit.label, icon), EditorStyles.label))
                        Ping(hit.gameObject);
                }
                if (sceneHits.Count == 0) EditorGUILayout.LabelField("No objects in the open scenes use it.", EditorStyles.miniLabel);

                EditorGUILayout.EndScrollView();
                EditorGUIUtility.SetIconSize(Vector2.zero);
            }

            GUILayout.FlexibleSpace();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(index == null ? "The index is built on the next search." : $"{indexedAssetCount} assets indexed", EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Rebuild Index", EditorStyles.miniButton))
                {
                    index = null;
                    if (searched) EditorApplication.delayCall += Search;
                }
            }
        }

        void ClearResults()
        {
            searched = false;
            assetHits.Clear();
            sceneHits.Clear();
        }

        void Search()
        {
            if (this == null) return;
            ClearResults();
            string path = target != null ? AssetDatabase.GetAssetPath(target) : null;
            if (string.IsNullOrEmpty(path)) return;
            if (index == null && !BuildIndex()) return;

            assetHits.AddRange(FindAssetsUsing(path));
            sceneHits.AddRange(FindInOpenScenes(path));
            searched = true;
            Repaint();
        }

        internal static List<string> FindAssetsUsing(string path)
        {
            if (index == null) BuildIndex();
            var users = index != null && index.TryGetValue(path, out var found) ? new List<string>(found) : new List<string>();
            users.Sort(EditorUtility.NaturalCompare);
            return users;
        }

        static bool BuildIndex()
        {
            var paths = AssetDatabase.GetAllAssetPaths();
            var newIndex = new Dictionary<string, List<string>>();
            int count = 0;
            try
            {
                for (int i = 0; i < paths.Length; i++)
                {
                    string path = paths[i];
                    if (!path.StartsWith("Assets/") || AssetDatabase.IsValidFolder(path)) continue;
                    if (count % 64 == 0 && EditorUtility.DisplayCancelableProgressBar("Find References", "Indexing " + path, (float)i / paths.Length))
                        return false;

                    count++;
                    foreach (string dependency in AssetDatabase.GetDependencies(path, false))
                    {
                        if (dependency == path) continue;
                        if (!newIndex.TryGetValue(dependency, out var users)) newIndex[dependency] = users = new List<string>();
                        users.Add(path);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            index = newIndex;
            indexedAssetCount = count;
            return true;
        }

        static List<SceneHit> FindInOpenScenes(string path)
        {
            var hits = new List<SceneHit>();
            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                var scene = SceneManager.GetSceneAt(s);
                if (!scene.isLoaded) continue;

                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    {
                        var go = t.gameObject;
                        string prefix = scene.name + " > " + HierarchyPath(t);
                        if (PrefabUtility.IsAnyPrefabInstanceRoot(go) && PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go) == path)
                            hits.Add(new SceneHit { gameObject = go, label = prefix + " (prefab instance)" });

                        foreach (var component in go.GetComponents<Component>())
                        {
                            if (component == null || component is Transform) continue;
                            if (ReferencesAsset(component, path))
                                hits.Add(new SceneHit { gameObject = go, label = $"{prefix} ({component.GetType().Name})" });
                        }
                    }
                }
            }
            return hits;
        }

        // Bookkeeping links every prefab instance has; they'd make each component of an instance look like a user.
        static readonly HashSet<string> IgnoredProperties = new HashSet<string> { "m_CorrespondingSourceObject", "m_PrefabInstance", "m_PrefabAsset", "m_GameObject" };

        static bool ReferencesAsset(Component component, string path)
        {
            var property = new SerializedObject(component).GetIterator();
            while (property.Next(true))
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference || IgnoredProperties.Contains(property.name)) continue;
                var value = property.objectReferenceValue;
                // Comparing paths also catches sub-assets, like a mesh inside a model or a sprite inside a texture.
                if (value != null && AssetDatabase.GetAssetPath(value) == path) return true;
            }
            return false;
        }

        static void Ping(Object obj)
        {
            if (obj == null) return;
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
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
