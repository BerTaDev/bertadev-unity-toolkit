using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BerTaDEV.Toolkit
{
    /// <summary>Saves modified scenes every few minutes. Off by default; turn it on in Preferences.</summary>
    [InitializeOnLoad]
    internal static class SceneAutoSave
    {
        static double nextSave;

        static SceneAutoSave()
        {
            Reschedule();
            EditorApplication.update += Update;
        }

        public static void Reschedule() => nextSave = EditorApplication.timeSinceStartup + ToolkitPrefs.AutoSaveMinutes * 60.0;

        static void Update()
        {
            if (EditorApplication.timeSinceStartup < nextSave) return;
            Reschedule();

            if (!ToolkitPrefs.AutoSaveEnabled) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;

            int saved = 0;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                // Untitled scenes would pop up a "Save As" dialog, so leave those alone.
                if (scene.isLoaded && scene.isDirty && !string.IsNullOrEmpty(scene.path) && EditorSceneManager.SaveScene(scene))
                    saved++;
            }

            if (saved > 0) Debug.Log($"[BerTaDEV] Auto-saved {saved} scene(s) at {DateTime.Now:HH:mm}.");
        }
    }
}
