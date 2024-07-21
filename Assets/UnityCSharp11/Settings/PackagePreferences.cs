using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityCSharp11.Settings
{
    public sealed class PackagePreferences : SettingsProvider
    {
        private readonly TemplateContainer _root;

        private static string PackagePath
        {
            get
            {
                string packagePath = FileUtil.GetPhysicalPath("Packages/com.alexejhero.unitycsharp11");
                // adjust path for local development
                if (packagePath == "Packages/com.alexejhero.unitycsharp11")
                    packagePath = FileUtil.GetPhysicalPath("Assets/UnityCSharp11");
                return packagePath;
            }
        }

        private PackagePreferences() : base("Preferences/C# 11", SettingsScope.User)
        {
            string uxmlPath = AssetDatabase.GUIDToAssetPath("bee79e4550d59f44a980f4620ac749e1");
            VisualTreeAsset doc = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            _root = doc.Instantiate();
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            rootElement.Add(_root);
            _root.Q<Button>("patch").clicked += () => ConfirmAndPatch(true);
            _root.Q<Button>("unpatch").clicked += () => ConfirmAndPatch(false);

            Toggle disableDialogToggle = _root.Q<Toggle>("suppress-unpatched-dialog");
            disableDialogToggle.value = !Config.ShowPatchDialog;
            disableDialogToggle.RegisterValueChangedCallback(evt => Config.ShowPatchDialog = !evt.newValue);
            Config.ShowPatchDialogChanged += ConfigOnShowPatchDialogChanged;

            UpdatePatched();
        }

        public override void OnDeactivate()
        {
            Config.ShowPatchDialogChanged -= ConfigOnShowPatchDialogChanged;
        }

        private void ConfigOnShowPatchDialogChanged(bool value)
        {
            _root.Q<Toggle>("suppress-unpatched-dialog").value = !value;
        }

        private void UpdatePatched()
        {
            bool isPatched = PatchChecker.IsPatched();
            string status = isPatched ? "patched" : "unpatched";
            _root.ClearClassList();
            _root.AddToClassList(status);

            string statusText = $"Unity {Application.unityVersion} is currently {(isPatched ? "patched" : "not patched")}.";
            _root.Q(status).Q<Label>("status-label").text = statusText;
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            return new PackagePreferences
            {
                keywords = new[] {"c#", "11", "namespace", "patch", "csharp", "langversion"},
            };
        }

        internal static void ConfirmAndPatch(bool patch)
        {
            bool confirmed = EditorUtility.DisplayDialog(
                patch ? "Patch Unity?" : "Unpatch Unity?",
                "Unity will need to be restarted to complete the process. Make sure you've saved your work before continuing.",
                "OK", "Cancel"
            );
            if (confirmed)
                RunPatcher(patch);
        }

        private static void RunPatcher(bool patch)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            string exePath = Path.Combine(PackagePath, "UnityPatcher~", "UnityPatcher.exe");

            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = $"\"{EditorApplication.applicationContentsPath}\" -{(patch ? 'p' : 'u')} \"{Application.dataPath}\"",
                UseShellExecute = true,
            });

            EditorApplication.Exit(0);
        }
    }
}
