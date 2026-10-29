using UnityEditor;
using UnityEngine;

namespace BerTaDEV.Toolkit
{
    /// <summary>
    /// Settings shared by every tool. Visual options live in EditorPrefs (per machine);
    /// options that change what happens to a project live in EditorUserSettings (per project).
    /// </summary>
    internal static class ToolkitPrefs
    {
        const string Prefix = "BerTaDEV.Toolkit.";

        public static bool HierarchyHeaders
        {
            get => EditorPrefs.GetBool(Prefix + "Hierarchy.Headers", true);
            set => EditorPrefs.SetBool(Prefix + "Hierarchy.Headers", value);
        }

        public static bool HierarchyToggles
        {
            get => EditorPrefs.GetBool(Prefix + "Hierarchy.Toggles", true);
            set => EditorPrefs.SetBool(Prefix + "Hierarchy.Toggles", value);
        }

        public static bool HierarchyIcons
        {
            get => EditorPrefs.GetBool(Prefix + "Hierarchy.Icons", true);
            set => EditorPrefs.SetBool(Prefix + "Hierarchy.Icons", value);
        }

        public static bool HierarchyZebra
        {
            get => EditorPrefs.GetBool(Prefix + "Hierarchy.Zebra", false);
            set => EditorPrefs.SetBool(Prefix + "Hierarchy.Zebra", value);
        }

        public static int ScreenshotSuperSize
        {
            get => Mathf.Clamp(EditorPrefs.GetInt(Prefix + "Screenshot.SuperSize", 1), 1, 4);
            set => EditorPrefs.SetInt(Prefix + "Screenshot.SuperSize", Mathf.Clamp(value, 1, 4));
        }

        public static int AutoSaveMinutes
        {
            get => Mathf.Max(1, EditorPrefs.GetInt(Prefix + "AutoSave.Minutes", 5));
            set => EditorPrefs.SetInt(Prefix + "AutoSave.Minutes", Mathf.Max(1, value));
        }

        public static bool AutoSaveEnabled
        {
            get => GetProjectBool("AutoSave.Enabled");
            set => SetProjectBool("AutoSave.Enabled", value);
        }

        public static bool PlayFromFirstScene
        {
            get => GetProjectBool("PlayFromFirstScene");
            set => SetProjectBool("PlayFromFirstScene", value);
        }

        static bool GetProjectBool(string key) => EditorUserSettings.GetConfigValue(Prefix + key) == "1";
        static void SetProjectBool(string key, bool value) => EditorUserSettings.SetConfigValue(Prefix + key, value ? "1" : "0");
    }
}
