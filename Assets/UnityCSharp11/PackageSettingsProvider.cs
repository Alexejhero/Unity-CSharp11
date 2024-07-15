using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;

namespace UnityCSharp11
{
    public sealed class PackageSettingsProvider : SettingsProvider
    {
        private class Styles
        {
            public static readonly GUIContent Enabled = new("Enable C# 11");
        }

        private PackageSettingsProvider() : base("Project/Editor/C# 11", SettingsScope.Project)
        {
        }

        public override void OnGUI(string searchContext)
        {
            bool value = EditorGUILayout.Toggle(Styles.Enabled, Configuration.IsEnabled);
            if (value != Configuration.IsEnabled)
            {
                Configuration.IsEnabled = value;
                EditorUtility.RequestScriptReload();
                CodeEditor.Editor.CurrentCodeEditor.SyncAll();
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            PackageSettingsProvider provider = new()
            {
                keywords = GetSearchKeywordsFromGUIContentProperties<Styles>()
            };

            return provider;
        }
    }
}
