using UnityEditor;
using UnityEngine;

namespace BerTaDEV.Toolkit
{
    /// <summary>Edit > Preferences > BerTaDEV Toolkit.</summary>
    internal static class ToolkitSettingsProvider
    {
        const string SettingsPath = "Preferences/BerTaDEV Toolkit";

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new SettingsProvider(SettingsPath, SettingsScope.User)
            {
                guiHandler = _ => OnGUI(),
                keywords = new[] { "Hierarchy", "Header", "Screenshot", "Auto Save", "Play Mode", "Scene" },
            };
        }

        [MenuItem("Tools/BerTaDEV/Preferences...", priority = 900)]
        static void Open() => SettingsService.OpenUserPreferences(SettingsPath);

        static void OnGUI()
        {
            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 220f;

            EditorGUILayout.LabelField("Hierarchy", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            bool headers = EditorGUILayout.Toggle(new GUIContent("Header rows", "GameObjects named like \"--- Name ---\" are drawn as section headers."), ToolkitPrefs.HierarchyHeaders);
            bool toggles = EditorGUILayout.Toggle(new GUIContent("Active toggles", "Checkbox on the right of each row to enable/disable the GameObject."), ToolkitPrefs.HierarchyToggles);
            bool icons = EditorGUILayout.Toggle(new GUIContent("Component icons", "Show the icons of the components on each GameObject."), ToolkitPrefs.HierarchyIcons);
            bool zebra = EditorGUILayout.Toggle(new GUIContent("Zebra rows", "Shade every other row."), ToolkitPrefs.HierarchyZebra);
            if (EditorGUI.EndChangeCheck())
            {
                ToolkitPrefs.HierarchyHeaders = headers;
                ToolkitPrefs.HierarchyToggles = toggles;
                ToolkitPrefs.HierarchyIcons = icons;
                ToolkitPrefs.HierarchyZebra = zebra;
                EditorApplication.RepaintHierarchyWindow();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Screenshots", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            int superSize = EditorGUILayout.IntPopup(new GUIContent("Resolution multiplier", "Screenshots are this many times larger than the Game view."),
                ToolkitPrefs.ScreenshotSuperSize,
                new[] { new GUIContent("1x"), new GUIContent("2x"), new GUIContent("3x"), new GUIContent("4x") },
                new[] { 1, 2, 3, 4 });
            if (EditorGUI.EndChangeCheck()) ToolkitPrefs.ScreenshotSuperSize = superSize;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scene Auto Save (this project)", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            bool autoSave = EditorGUILayout.Toggle(new GUIContent("Enabled", "Saves modified scenes that already have a file. Never saves in Play Mode."), ToolkitPrefs.AutoSaveEnabled);
            int minutes = EditorGUILayout.IntSlider("Interval (minutes)", ToolkitPrefs.AutoSaveMinutes, 1, 60);
            if (EditorGUI.EndChangeCheck())
            {
                ToolkitPrefs.AutoSaveEnabled = autoSave;
                ToolkitPrefs.AutoSaveMinutes = minutes;
                SceneAutoSave.Reschedule();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Play Mode (this project)", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            bool playFromFirst = EditorGUILayout.Toggle(new GUIContent("Play from first build scene", "Enter Play Mode from build scene #0, then come back to the scenes you had open."), PlayFromFirstScene.Enabled);
            if (EditorGUI.EndChangeCheck()) PlayFromFirstScene.Enabled = playFromFirst;

            EditorGUIUtility.labelWidth = previousLabelWidth;
        }
    }
}
