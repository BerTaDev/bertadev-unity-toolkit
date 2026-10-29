using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BerTaDEV.Toolkit
{
    /// <summary>Drag-and-drop list of the assets you use all the time. Saved per project and per user.</summary>
    internal sealed class FavoritesWindow : EditorWindow
    {
        const string ConfigKey = "BerTaDEV.Toolkit.Favorites";
        const string DragKey = "BerTaDEV.Toolkit.FavoritesDrag";

        readonly List<string> guids = new List<string>();
        Vector2 scroll;
        int pressedIndex = -1;

        static Color SelectedRowColor => EditorGUIUtility.isProSkin ? new Color(0.17f, 0.36f, 0.53f) : new Color(0.23f, 0.45f, 0.69f, 0.45f);

        [MenuItem("Tools/BerTaDEV/Favorites", priority = 106)]
        static void Open()
        {
            var window = GetWindow<FavoritesWindow>();
            window.titleContent = new GUIContent("Favorites", EditorGUIUtility.IconContent("Favorite Icon").image);
            window.minSize = new Vector2(220f, 150f);
        }

        [MenuItem("Assets/BerTaDEV/Add To Favorites", priority = 31)]
        static void AddSelectionFromMenu()
        {
            Open();
            GetWindow<FavoritesWindow>().Add(Selection.objects);
        }

        [MenuItem("Assets/BerTaDEV/Add To Favorites", true)]
        static bool CanAddSelection() => Selection.objects.Any(EditorUtility.IsPersistent);

        void OnEnable()
        {
            guids.Clear();
            string saved = EditorUserSettings.GetConfigValue(ConfigKey);
            if (!string.IsNullOrEmpty(saved)) guids.AddRange(saved.Split(';'));
        }

        void Save()
        {
            EditorUserSettings.SetConfigValue(ConfigKey, string.Join(";", guids));
            Repaint();
        }

        internal void Add(IEnumerable<Object> objects)
        {
            foreach (var obj in objects)
            {
                if (obj == null || !EditorUtility.IsPersistent(obj)) continue;
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
                if (!string.IsNullOrEmpty(guid) && !guids.Contains(guid)) guids.Add(guid);
            }
            Save();
        }

        void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Add Selection", EditorStyles.toolbarButton)) Add(Selection.objects);
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(guids.Count == 0))
                {
                    if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
                        EditorApplication.delayCall += ConfirmClear;
                }
            }

            if (guids.Count == 0)
                EditorGUILayout.HelpBox("Drag assets or folders here to keep them one click away.", MessageType.Info);

            int removeAt = -1;
            var minus = EditorGUIUtility.IconContent("Toolbar Minus");
            EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
            scroll = EditorGUILayout.BeginScrollView(scroll);
            for (int i = 0; i < guids.Count; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadMainAssetAtPath(path);

                var row = GUILayoutUtility.GetRect(0f, 22f, GUILayout.ExpandWidth(true));
                var labelRect = new Rect(row.x + 4f, row.y, row.width - 30f, row.height);
                var removeRect = new Rect(row.xMax - 24f, row.y + 2f, 22f, 18f);

                if (asset != null && Selection.activeObject == asset && Event.current.type == EventType.Repaint)
                    EditorGUI.DrawRect(row, SelectedRowColor);

                if (asset != null)
                {
                    GUI.Label(labelRect, new GUIContent(asset.name, AssetDatabase.GetCachedIcon(path), path));
                }
                else
                {
                    using (new EditorGUI.DisabledScope(true))
                        GUI.Label(labelRect, new GUIContent("(missing)", "This asset was deleted or moved out of the project."));
                }

                if (GUI.Button(removeRect, new GUIContent(minus.image, "Remove from favorites"), GUIStyle.none)) removeAt = i;
                HandleRowEvents(i, labelRect, asset);
            }
            EditorGUILayout.EndScrollView();
            EditorGUIUtility.SetIconSize(Vector2.zero);

            GUILayout.Label("Drag assets in · double-click to open · drag out to use", EditorStyles.centeredGreyMiniLabel);

            if (removeAt >= 0)
            {
                guids.RemoveAt(removeAt);
                Save();
            }
            HandleDrop();
        }

        void HandleRowEvents(int index, Rect rect, Object asset)
        {
            var evt = Event.current;
            if (evt.type == EventType.MouseUp) pressedIndex = -1;
            if (asset == null || !rect.Contains(evt.mousePosition)) return;

            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                if (evt.clickCount == 2)
                {
                    AssetDatabase.OpenAsset(asset);
                }
                else
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
                pressedIndex = index;
                evt.Use();
            }
            else if (evt.type == EventType.MouseDrag && pressedIndex == index)
            {
                // Drag a favorite out into the Scene, the Inspector or another window.
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new[] { asset };
                DragAndDrop.paths = new[] { AssetDatabase.GetAssetPath(asset) };
                DragAndDrop.SetGenericData(DragKey, true);
                DragAndDrop.StartDrag(asset.name);
                pressedIndex = -1;
                evt.Use();
            }
        }

        void HandleDrop()
        {
            var evt = Event.current;
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform) return;
            if (DragAndDrop.GetGenericData(DragKey) != null) return; // started from this list

            bool hasAssets = DragAndDrop.objectReferences.Any(o => o != null && EditorUtility.IsPersistent(o));
            DragAndDrop.visualMode = hasAssets ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
            if (hasAssets && evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                Add(DragAndDrop.objectReferences);
            }
            evt.Use();
        }

        void ConfirmClear()
        {
            if (this == null || !EditorUtility.DisplayDialog("Clear favorites", "Remove every favorite from this list?", "Clear", "Cancel")) return;
            guids.Clear();
            Save();
        }
    }
}
