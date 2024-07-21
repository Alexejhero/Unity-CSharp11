using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityCSharp11.Settings.Dialogs
{
    public sealed class LaunchDialog : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset uxml;

        private void Awake()
        {
            string uxmlPath = AssetDatabase.GUIDToAssetPath("861861816589fc840b37b3859c40a370");
            uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            if (!SessionState.GetBool("UnityCSharp11.FirstLaunch", true)) return;
            SessionState.SetBool("UnityCSharp11.FirstLaunch", false);

            EditorApplication.delayCall += () =>
            {
                if (Config.ShowPatchDialog && !PatchChecker.IsPatched())
                    OpenPatchDialog();
            };
        }

        //[MenuItem("Window/UI Toolkit/PatchDialog")] // debugging
        private static void OpenPatchDialog()
        {
            LaunchDialog wnd = GetWindow<LaunchDialog>();
            wnd.minSize = wnd.maxSize = new Vector2(350, 150);
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

            Toggle dontShowAgainCheckbox = content.Q<Toggle>("dont-show-again");
            Button patchButton = content.Q<Button>("patch");
            Button ignoreButton = content.Q<Button>("ignore");

            dontShowAgainCheckbox.value = false;
            dontShowAgainCheckbox.RegisterValueChangedCallback(evt =>
            {
                patchButton.SetEnabled(!evt.newValue);
            });

            patchButton.clicked += () =>
            {
                PackagePreferences.ConfirmAndPatch(true);
                Close();
            };

            ignoreButton.clicked += () =>
            {
                if (dontShowAgainCheckbox.value)
                    Config.ShowPatchDialog = false;
                Close();
            };
        }
    }
}
