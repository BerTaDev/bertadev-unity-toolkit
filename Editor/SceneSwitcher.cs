using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BerTaDEV.Toolkit
{
    /// <summary>Lists every scene in the project for one-click opening, additive loading and playing.</summary>
    internal sealed class SceneSwitcher : EditorWindow
    {
        [SerializeField] string search = "";
        [SerializeField] bool buildScenesOnly;
        Vector2 scroll;
        List<string> scenePaths;

        [MenuItem("Tools/BerTaDEV/Scene Switcher", priority = 100)]
        static void Open()
        {
            var window = GetWindow<SceneSwitcher>();
            window.titleContent = new GUIContent("Scenes", EditorGUIUtility.IconContent("SceneAsset Icon").image);
            window.minSize = new Vector2(260f, 150f);
        }

        void OnEnable()
        {
            EditorApplication.projectChanged += OnProjectChanged;
            Refresh();
        }

        void OnDisable() => EditorApplication.projectChanged -= OnProjectChanged;

        void OnProjectChanged()
        {
            Refresh();
            Repaint();
        }

        void Refresh()
        {
            scenePaths = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" })
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Distinct()
                .OrderBy(path => Path.GetFileNameWithoutExtension(path), StringComparer.OrdinalIgnoreCase)
                .ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        void OnGUI()
        {
            if (scenePaths == null) Refresh();

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                search = EditorGUILayout.TextField(search, EditorStyles.toolbarSearchField, GUILayout.MinWidth(60f));
                buildScenesOnly = GUILayout.Toggle(buildScenesOnly, new GUIContent("In Build", "Only show scenes in the build list"), EditorStyles.toolbarButton, GUILayout.Width(56f));

                bool playFromFirst = GUILayout.Toggle(PlayFromFirstScene.Enabled,
                    new GUIContent("Play #0", "Always enter Play Mode from the first build scene"), EditorStyles.toolbarButton, GUILayout.Width(52f));
                if (playFromFirst != PlayFromFirstScene.Enabled) PlayFromFirstScene.Enabled = playFromFirst;

                if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh"), EditorStyles.toolbarButton, GUILayout.Width(26f)))
                    Refresh();
            }

            var buildScenes = EditorBuildSettings.scenes;
            string activePath = SceneManager.GetActiveScene().path;
            var playIcon = EditorGUIUtility.IconContent("PlayButton").image;

            scroll = EditorGUILayout.BeginScrollView(scroll);
            int shown = 0;
            foreach (string path in scenePaths)
            {
                string name = Path.GetFileNameWithoutExtension(path);
                if (!string.IsNullOrEmpty(search) && name.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;

                int buildIndex = BuildIndexOf(buildScenes, path);
                if (buildScenesOnly && buildIndex < 0) continue;

                DrawRow(path, name, buildIndex, path == activePath, playIcon);
                shown++;
            }
            if (shown == 0) EditorGUILayout.HelpBox("No scenes match.", MessageType.None);
            EditorGUILayout.EndScrollView();
        }

        void DrawRow(string path, string name, int buildIndex, bool isActive, Texture playIcon)
        {
            var row = EditorGUILayout.BeginHorizontal();

            var nameContent = new GUIContent(name, path);
            if (GUILayout.Button(nameContent, isActive ? EditorStyles.boldLabel : EditorStyles.label, GUILayout.MinWidth(60f)))
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(path));

            GUILayout.Label(buildIndex >= 0 ? "#" + buildIndex : string.Empty, EditorStyles.miniLabel, GUILayout.Width(26f));

            if (GUILayout.Button(new GUIContent("Open", "Open this scene"), EditorStyles.miniButtonLeft, GUILayout.Width(42f)))
                EditorApplication.delayCall += () => OpenScene(path, OpenSceneMode.Single, false);
            using (new EditorGUI.DisabledScope(IsLoaded(path)))
            {
                if (GUILayout.Button(new GUIContent("+", "Open additively"), EditorStyles.miniButtonMid, GUILayout.Width(22f)))
                    EditorApplication.delayCall += () => OpenScene(path, OpenSceneMode.Additive, false);
            }
            if (GUILayout.Button(new GUIContent(playIcon, "Open and enter Play Mode"), EditorStyles.miniButtonRight, GUILayout.Width(26f)))
                EditorApplication.delayCall += () => OpenScene(path, OpenSceneMode.Single, true);

            EditorGUILayout.EndHorizontal();

            if (Event.current.type == EventType.ContextClick && row.Contains(Event.current.mousePosition))
            {
                ShowContextMenu(path, buildIndex >= 0 || IsInBuildList(path));
                Event.current.Use();
            }
        }

        static void ShowContextMenu(string path, bool inBuild)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Ping"), false, () => EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)));
            menu.AddItem(new GUIContent(inBuild ? "Remove From Build" : "Add To Build"), false, () => SetInBuild(path, !inBuild));
            menu.AddItem(new GUIContent("Show In Explorer"), false, () => EditorUtility.RevealInFinder(path));
            menu.ShowAsContext();
        }

        static void OpenScene(string path, OpenSceneMode mode, bool play)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[BerTaDEV] Exit Play Mode before switching scenes.");
                return;
            }
            if (mode == OpenSceneMode.Single && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorSceneManager.OpenScene(path, mode);
            if (!play) return;

            // The play button means "play this scene", even when Play From First Build Scene is on.
            // PlayFromFirstScene puts its start scene back once we return to Edit Mode.
            EditorSceneManager.playModeStartScene = null;
            EditorApplication.EnterPlaymode();
        }

        static bool IsLoaded(string path)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && scene.path == path) return true;
            }
            return false;
        }

        static int BuildIndexOf(EditorBuildSettingsScene[] buildScenes, string path)
        {
            int index = 0;
            foreach (var scene in buildScenes)
            {
                if (!scene.enabled) continue;
                if (scene.path == path) return index;
                index++;
            }
            return -1;
        }

        static bool IsInBuildList(string path) => EditorBuildSettings.scenes.Any(scene => scene.path == path);

        static void SetInBuild(string path, bool include)
        {
            var scenes = EditorBuildSettings.scenes.Where(scene => scene.path != path).ToList();
            if (include) scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }

    /// <summary>Optionally makes Play Mode always start from the first enabled scene in the build list.</summary>
    [InitializeOnLoad]
    internal static class PlayFromFirstScene
    {
        const string MenuPath = "Tools/BerTaDEV/Play From First Build Scene";

        static PlayFromFirstScene()
        {
            EditorApplication.delayCall += Apply;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode) Apply();
            };
            EditorBuildSettings.sceneListChanged += Apply;
        }

        public static bool Enabled
        {
            get => ToolkitPrefs.PlayFromFirstScene;
            set
            {
                ToolkitPrefs.PlayFromFirstScene = value;
                if (value) Apply();
                else EditorSceneManager.playModeStartScene = null;
            }
        }

        [MenuItem(MenuPath, priority = 500)]
        static void Toggle() => Enabled = !Enabled;

        [MenuItem(MenuPath, true)]
        static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, Enabled);
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }

        static void Apply()
        {
            if (!Enabled) return;
            var first = EditorBuildSettings.scenes.FirstOrDefault(scene => scene.enabled);
            EditorSceneManager.playModeStartScene = first != null ? AssetDatabase.LoadAssetAtPath<SceneAsset>(first.path) : null;
        }
    }
}
