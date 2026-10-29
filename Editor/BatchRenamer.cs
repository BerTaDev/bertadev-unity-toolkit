using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace BerTaDEV.Toolkit
{
    /// <summary>Renames the selected GameObjects or assets with find/replace, prefix/suffix and numbering.</summary>
    internal sealed class BatchRenamer : EditorWindow
    {
        enum CaseMode { Keep, Upper, Lower }

        class Target
        {
            public Object obj;
            public string assetPath; // null for scene objects
            public string oldName;
        }

        [SerializeField] string baseName = "";
        [SerializeField] string find = "";
        [SerializeField] string replace = "";
        [SerializeField] bool useRegex;
        [SerializeField] string prefix = "";
        [SerializeField] string suffix = "";
        [SerializeField] bool numbered;
        [SerializeField] string separator = "_";
        [SerializeField] int startNumber = 1;
        [SerializeField] int step = 1;
        [SerializeField] int digits = 2;
        [SerializeField] CaseMode caseMode;
        Vector2 scroll;

        [MenuItem("Tools/BerTaDEV/Batch Renamer", priority = 102)]
        static void Open()
        {
            var window = GetWindow<BatchRenamer>();
            window.titleContent = new GUIContent("Batch Renamer");
            window.minSize = new Vector2(340f, 380f);
        }

        void OnSelectionChange() => Repaint();

        void OnGUI()
        {
            EditorGUILayout.Space(4f);
            baseName = EditorGUILayout.TextField(new GUIContent("Base Name", "Replaces the whole name. Leave empty to keep the original name."), baseName);
            using (new EditorGUILayout.HorizontalScope())
            {
                find = EditorGUILayout.TextField("Find", find);
                useRegex = GUILayout.Toggle(useRegex, new GUIContent(".*", "Use a regular expression"), EditorStyles.miniButton, GUILayout.Width(28f));
            }
            replace = EditorGUILayout.TextField("Replace With", replace);
            prefix = EditorGUILayout.TextField("Prefix", prefix);
            suffix = EditorGUILayout.TextField("Suffix", suffix);

            numbered = EditorGUILayout.Toggle("Add Numbers", numbered);
            using (new EditorGUI.DisabledScope(!numbered))
            {
                EditorGUI.indentLevel++;
                separator = EditorGUILayout.TextField("Separator", separator);
                startNumber = EditorGUILayout.IntField("Start At", startNumber);
                step = EditorGUILayout.IntField("Step", step);
                digits = EditorGUILayout.IntSlider("Digits", digits, 1, 6);
                EditorGUI.indentLevel--;
            }
            caseMode = (CaseMode)EditorGUILayout.EnumPopup("Case", caseMode);

            var targets = GetTargets();
            var newNames = BuildNames(targets, out string error);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Preview ({targets.Count} selected)", EditorStyles.boldLabel);

            if (error != null)
            {
                EditorGUILayout.HelpBox(error, MessageType.Error);
            }
            else if (targets.Count == 0)
            {
                EditorGUILayout.HelpBox("Select GameObjects in the Hierarchy or assets in the Project window.", MessageType.Info);
            }
            else
            {
                scroll = EditorGUILayout.BeginScrollView(scroll, EditorStyles.helpBox, GUILayout.ExpandHeight(true));
                float half = (position.width - 60f) / 2f;
                for (int i = 0; i < targets.Count; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label(targets[i].oldName, GUILayout.Width(half));
                        GUILayout.Label("→", GUILayout.Width(16f));
                        GUILayout.Label(newNames[i], newNames[i] == targets[i].oldName ? EditorStyles.label : EditorStyles.boldLabel);
                    }
                }
                EditorGUILayout.EndScrollView();
            }

            GUILayout.FlexibleSpace();
            using (new EditorGUI.DisabledScope(error != null || targets.Count == 0))
            {
                if (GUILayout.Button("Rename", GUILayout.Height(28f)))
                    Apply(targets, newNames);
            }
            EditorGUILayout.Space(4f);
        }

        List<string> BuildNames(List<Target> targets, out string error)
        {
            error = null;
            var names = new List<string>(targets.Count);

            Regex regex = null;
            if (useRegex && !string.IsNullOrEmpty(find))
            {
                try { regex = new Regex(find, RegexOptions.None, TimeSpan.FromMilliseconds(100)); }
                catch (ArgumentException ex) { error = "Invalid regular expression: " + ex.Message; return names; }
            }

            for (int i = 0; i < targets.Count; i++)
            {
                string name = string.IsNullOrEmpty(baseName) ? targets[i].oldName : baseName;
                if (!string.IsNullOrEmpty(find))
                {
                    try { name = regex != null ? regex.Replace(name, replace) : name.Replace(find, replace); }
                    catch (RegexMatchTimeoutException) { error = "The regular expression took too long to run."; return names; }
                }

                name = prefix + name + suffix;
                if (numbered) name += separator + (startNumber + i * step).ToString().PadLeft(digits, '0');

                if (caseMode == CaseMode.Upper) name = name.ToUpperInvariant();
                else if (caseMode == CaseMode.Lower) name = name.ToLowerInvariant();

                if (string.IsNullOrWhiteSpace(name)) error = "Names can't be empty.";
                else if (targets[i].assetPath != null && name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                    error = $"'{name}' contains characters that aren't allowed in file names.";

                names.Add(name);
            }
            return names;
        }

        static List<Target> GetTargets()
        {
            var sceneObjects = new List<GameObject>();
            var assets = new List<Target>();

            foreach (var obj in Selection.objects)
            {
                if (obj == null) continue;
                if (EditorUtility.IsPersistent(obj))
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    if (!path.StartsWith("Assets/") || !AssetDatabase.IsMainAsset(obj)) continue;
                    string name = AssetDatabase.IsValidFolder(path) ? Path.GetFileName(path) : Path.GetFileNameWithoutExtension(path);
                    assets.Add(new Target { obj = obj, assetPath = path, oldName = name });
                }
                else if (obj is GameObject go)
                {
                    sceneObjects.Add(go);
                }
            }

            // Number things in the order you see them: hierarchy order for scene objects, natural path order for assets.
            sceneObjects.Sort(CompareHierarchyOrder);
            assets.Sort((a, b) => EditorUtility.NaturalCompare(a.assetPath, b.assetPath));

            var targets = new List<Target>(sceneObjects.Count + assets.Count);
            foreach (var go in sceneObjects) targets.Add(new Target { obj = go, oldName = go.name });
            targets.AddRange(assets);
            return targets;
        }

        static void Apply(List<Target> targets, List<string> newNames)
        {
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            var errors = new List<string>();

            for (int i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target.oldName == newNames[i] || target.obj == null) continue;

                if (target.assetPath != null)
                {
                    string error = AssetDatabase.RenameAsset(target.assetPath, newNames[i]);
                    if (!string.IsNullOrEmpty(error)) errors.Add(error);
                }
                else
                {
                    Undo.RecordObject(target.obj, "Batch Rename");
                    target.obj.name = newNames[i];
                }
            }

            Undo.SetCurrentGroupName("Batch Rename");
            Undo.CollapseUndoOperations(group);

            if (errors.Count > 0)
                Debug.LogWarning("[BerTaDEV] Some assets could not be renamed:\n" + string.Join("\n", errors));
        }

        static int CompareHierarchyOrder(GameObject a, GameObject b)
        {
            var keyA = OrderKey(a.transform);
            var keyB = OrderKey(b.transform);
            for (int i = 0; i < keyA.Count && i < keyB.Count; i++)
                if (keyA[i] != keyB[i]) return keyA[i].CompareTo(keyB[i]);
            return keyA.Count.CompareTo(keyB.Count);
        }

        static List<int> OrderKey(Transform t)
        {
            var key = new List<int>();
            for (var current = t; current != null; current = current.parent)
                key.Add(current.GetSiblingIndex());
            key.Add(SceneOrder(t.gameObject.scene));
            key.Reverse();
            return key;
        }

        static int SceneOrder(Scene scene)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i) == scene) return i;
            return int.MaxValue;
        }
    }
}
