using UnityEditor;
using UnityEngine;

namespace BerTaDEV.Toolkit
{
    /// <summary>Draws fields marked with <see cref="ReadOnlyInspectorAttribute"/> greyed out.</summary>
    [CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
    internal sealed class ReadOnlyInspectorDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool isStruct = property.propertyType == SerializedPropertyType.Generic && property.hasVisibleChildren;
            if (!isStruct)
            {
                using (new EditorGUI.DisabledScope(true))
                    EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            // Keep the foldout clickable so you can still look inside; only the values are locked.
            var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(line, property.isExpanded, label, true);
            if (!property.isExpanded) return;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.indentLevel++;
                float y = line.yMax + EditorGUIUtility.standardVerticalSpacing;
                var child = property.Copy();
                var end = property.GetEndProperty();
                for (bool enterChildren = true; child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end); enterChildren = false)
                {
                    float height = EditorGUI.GetPropertyHeight(child, true);
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), child, true);
                    y += height + EditorGUIUtility.standardVerticalSpacing;
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}
