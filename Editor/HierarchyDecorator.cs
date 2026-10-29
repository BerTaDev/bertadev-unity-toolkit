using UnityEditor;
using UnityEngine;

namespace BerTaDEV.Toolkit
{
    /// <summary>
    /// Draws section headers, active toggles and component icons in the Hierarchy window.
    /// Name a GameObject like "--- Environment ---" to turn it into a header row.
    /// </summary>
    [InitializeOnLoad]
    internal static class HierarchyDecorator
    {
        const string HeaderPrefix = "---";
        const float IconSize = 16f;
        const float PrefabArrowWidth = 14f;
        const int MaxIcons = 4;

        static GUIStyle headerStyle;
        static Texture missingScriptIcon;

        static HierarchyDecorator()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnItemGUI;
        }

        static Color HeaderColor => EditorGUIUtility.isProSkin ? new Color(0.13f, 0.13f, 0.13f) : new Color(0.68f, 0.68f, 0.68f);
        static Color SelectedHeaderColor => EditorGUIUtility.isProSkin ? new Color(0.17f, 0.36f, 0.53f) : new Color(0.23f, 0.45f, 0.69f);
        static Color ZebraColor => EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.025f) : new Color(0f, 0f, 0f, 0.04f);

        static void OnItemGUI(int instanceID, Rect rect)
        {
            // Scene header rows pass a scene handle here, so this is null for them.
#if UNITY_6000_3_OR_NEWER
            var go = EditorUtility.EntityIdToObject(instanceID) as GameObject;
#else
            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
#endif
            if (go == null) return;

            if (ToolkitPrefs.HierarchyZebra && Mathf.RoundToInt(rect.y / rect.height) % 2 == 1)
                EditorGUI.DrawRect(new Rect(0f, rect.y, rect.xMax + IconSize, rect.height), ZebraColor);

            if (ToolkitPrefs.HierarchyHeaders && go.name.StartsWith(HeaderPrefix))
            {
                DrawHeader(rect, go);
                return;
            }

            float x = rect.xMax;
            if (PrefabUtility.IsAnyPrefabInstanceRoot(go)) x -= PrefabArrowWidth;

            if (ToolkitPrefs.HierarchyToggles)
            {
                x -= IconSize;
                DrawActiveToggle(new Rect(x, rect.y, IconSize, rect.height), go);
            }

            if (ToolkitPrefs.HierarchyIcons) DrawComponentIcons(x, rect, go);
        }

        static void DrawHeader(Rect rect, GameObject go)
        {
            bool selected = Selection.Contains(go);
            EditorGUI.DrawRect(rect, selected ? SelectedHeaderColor : HeaderColor);

            if (headerStyle == null)
                headerStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };

            var textColor = selected || EditorGUIUtility.isProSkin ? Color.white : Color.black;
            if (!go.activeInHierarchy) textColor.a = 0.45f;
            headerStyle.normal.textColor = textColor;

            GUI.Label(rect, go.name.Trim('-', ' '), headerStyle);
        }

        static void DrawActiveToggle(Rect rect, GameObject go)
        {
            EditorGUI.BeginChangeCheck();
            bool active = GUI.Toggle(rect, go.activeSelf, GUIContent.none);
            if (!EditorGUI.EndChangeCheck()) return;

            // Toggling a selected object toggles the whole selection, like the Inspector does.
            var targets = Selection.Contains(go) ? Selection.gameObjects : new[] { go };
            Undo.RecordObjects(targets, active ? "Activate GameObjects" : "Deactivate GameObjects");
            foreach (var target in targets) target.SetActive(active);
        }

        static void DrawComponentIcons(float x, Rect rect, GameObject go)
        {
            var components = go.GetComponents<Component>();
            int drawn = 0;

            for (int i = components.Length - 1; i >= 0 && drawn < MaxIcons; i--)
            {
                var component = components[i];
                if (component is Transform) continue;

                Texture icon;
                string tooltip;
                if (component == null)
                {
                    if (missingScriptIcon == null) missingScriptIcon = EditorGUIUtility.IconContent("console.warnicon.sml").image;
                    icon = missingScriptIcon;
                    tooltip = "Missing script";
                }
                else
                {
                    icon = AssetPreview.GetMiniThumbnail(component);
                    tooltip = ObjectNames.NicifyVariableName(component.GetType().Name);
                }
                if (icon == null) continue;

                x -= IconSize;
                var iconRect = new Rect(x, rect.y, IconSize, rect.height);

                var previousColor = GUI.color;
                if (component != null && EditorUtility.GetObjectEnabled(component) == 0)
                    GUI.color = new Color(1f, 1f, 1f, 0.35f);
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                GUI.color = previousColor;

                GUI.Label(iconRect, new GUIContent(string.Empty, tooltip));
                drawn++;
            }
        }

        [MenuItem("GameObject/BerTaDEV/Hierarchy Header", false, 10)]
        static void CreateHeader(MenuCommand command)
        {
            var go = new GameObject("--- HEADER ---");
            GameObjectUtility.SetParentAndAlign(go, command.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create Hierarchy Header");
            Selection.activeObject = go;
        }
    }
}
