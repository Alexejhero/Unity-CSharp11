using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityCSharp11.Settings.Dialogs
{
    public sealed class PatchDialog : EditorWindow
    {
        private const string IgnoreFile = "Library/com.alexejhero.unitycsharp11/ignore";
        private static readonly HashSet<string> _ignoredVersions = new();
        
        [SerializeField]
        private VisualTreeAsset uxml;

        private void Awake()
        {
            string uxmlPath = AssetDatabase.GUIDToAssetPath("861861816589fc840b37b3859c40a370");
            uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
        }
        public static bool ShowPatchDialog
        {
            get => !_ignoredVersions.Contains(Application.unityVersion);
            set
            {
                if (ShowPatchDialog == value) return;
                if (value)
                    _ignoredVersions.Remove(Application.unityVersion);
                else
                    _ignoredVersions.Add(Application.unityVersion);

                using FileStream fs = File.Open(IgnoreFile, FileMode.Truncate, FileAccess.Write);
                using StreamWriter writer = new StreamWriter(fs, Encoding.ASCII);
                foreach (string version in _ignoredVersions)
                    writer.WriteLine(version);
                
                // anti-binding propaganda
                if (PackageSettingsProvider.Instance is { } settings)
                {
                    settings.OnShowPatchDialogUpdated();
                }
            }
        }
        [InitializeOnLoadMethod]
        private static void Init()
        {
            ReadIgnoreFile();

            if (!SessionState.GetBool("UnityCSharp11.FirstLaunch", true)) return;
            SessionState.SetBool("UnityCSharp11.FirstLaunch", false);
            
            EditorApplication.delayCall += () =>
            {
                if (ShowPatchDialog && !PatchChecker.IsPatched())
                    OpenPatchDialog();
            };
        }

        private static void ReadIgnoreFile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(IgnoreFile)!);

            File.OpenWrite(IgnoreFile).Close();
            foreach (var line in File.ReadAllLines(IgnoreFile))
                _ignoredVersions.Add(line);
        }

        //[MenuItem("Window/UI Toolkit/PatchDialog")] // debugging
        private static void OpenPatchDialog()
        {
            PatchDialog wnd = GetWindow<PatchDialog>();
            wnd.minSize = wnd.maxSize = new(350, 150);
            wnd.titleContent = new GUIContent("Unity C# 11");
        }

        public void CreateGUI()
        {
            // someone may have left the window open then patched, unity will restore the window
            if (PatchChecker.IsPatched())
            {
                Close();
                return;
            }
            VisualElement root = rootVisualElement;

            VisualElement content = uxml.Instantiate();
            root.Add(content);

            Toggle checkbox = content.Q<Toggle>("dont-show-again");
            checkbox.value = !ShowPatchDialog; // technically this can only ever get shown when unchecked
            checkbox.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                ShowPatchDialog = !evt.newValue;
            });
            content.Q<Button>("patch").clicked += () =>
            {
                PackageSettingsProvider.ConfirmAndPatch(true);
                Close();
            };
            content.Q<Button>("ignore").clicked += Close;
        }
    }
}