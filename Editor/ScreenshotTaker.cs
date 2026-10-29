using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BerTaDEV.Toolkit
{
    /// <summary>Saves Game view screenshots to &lt;Project&gt;/Screenshots (F12).</summary>
    internal static class ScreenshotTaker
    {
        static string Folder => Path.Combine(Path.GetDirectoryName(Application.dataPath), "Screenshots");

        [MenuItem("Tools/BerTaDEV/Screenshot/Capture Game View _F12", priority = 400)]
        static void Capture()
        {
            Directory.CreateDirectory(Folder);

            int superSize = ToolkitPrefs.ScreenshotSuperSize;
            string product = string.Concat(Application.productName.Split(Path.GetInvalidFileNameChars()));
            string file = Path.Combine(Folder, $"{product}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}{(superSize > 1 ? $"@{superSize}x" : "")}.png");

            ScreenCapture.CaptureScreenshot(file, superSize);

            // The capture happens on the Game view's next render, so make sure one is coming.
            var gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            if (gameViewType != null) EditorWindow.GetWindow(gameViewType, false, null, false).Repaint();

            ReportWhenWritten(file);
        }

        [MenuItem("Tools/BerTaDEV/Screenshot/Open Screenshots Folder", priority = 401)]
        static void OpenFolder()
        {
            Directory.CreateDirectory(Folder);
            Application.OpenURL(new Uri(Folder).AbsoluteUri);
        }

        static void ReportWhenWritten(string file)
        {
            double deadline = EditorApplication.timeSinceStartup + 5.0;
            EditorApplication.CallbackFunction poll = null;
            poll = () =>
            {
                if (File.Exists(file))
                {
                    EditorApplication.update -= poll;
                    Debug.Log($"[BerTaDEV] Screenshot saved: {file}");
                }
                else if (EditorApplication.timeSinceStartup > deadline)
                {
                    EditorApplication.update -= poll;
                    Debug.LogWarning("[BerTaDEV] Screenshot was not written. Make sure the Game view is open and visible.");
                }
            };
            EditorApplication.update += poll;
        }
    }
}
