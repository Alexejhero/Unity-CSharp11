using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityCSharp11
{
    public sealed class PackageSettingsProvider : SettingsProvider
    {
        private static readonly GUIStyle _wrappingText = new(EditorStyles.label)
        {
            wordWrap = true
        };
        private static readonly GUIStyle _greenText = new(EditorStyles.label)
        {
            normal = { textColor = Color.green },
            hover = { textColor = Color.green },
        };
        private static readonly GUIStyle _redText = new(EditorStyles.label)
        {
            normal = { textColor = Color.red },
            hover = { textColor = Color.red }
        };

        private bool _patched;

        private PackageSettingsProvider() : base("Preferences/C# 11", SettingsScope.User)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _patched = PatchChecker.IsPatched();
        }

        public override void OnGUI(string searchContext)
        {
            GUILayout.Label("File-scoped Namespaces", EditorStyles.boldLabel);
            GUILayout.Label("Unity does not natively support MonoBehaviours or ScriptableObjects in files with file-scoped namespaces, and will refuse to load them. In order to use this feature in Unity, an editor patch is required.", _wrappingText);
            GUILayout.Space(10);
            if (_patched)
            {
                GUILayout.Label($"Unity {Application.unityVersion} is patched.", _greenText);
                GUILayout.Label("You now have access to use file-scoped namespaces in MonoBehaviour and ScriptableObject files.", _wrappingText);
                GUILayout.Space(10);
                GUILayout.Label("This patch does not affect the runtime in any way, and it also doesn't break other projects which don't use this package. As such, once you apply it, there is no real need to ever remove it. However, if you want to, you can still do so.", _wrappingText);
                if (GUILayout.Button("Unpatch"))
                {
                    if (EditorUtility.DisplayDialog("Unpatch Unity?", "Unity needs to be restarted in order for the patch to be removed. Save your work before continuing.", "OK", "Cancel"))
                    {
                        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

                        string packagePath = FileUtil.GetPhysicalPath("Packages/com.alexejhero.unitycsharp11");
                        if (packagePath == "Packages/com.alexejhero.unitycsharp11") packagePath = FileUtil.GetPhysicalPath("Assets");
                        string exePath = Path.Combine(packagePath, "UnityCSharp11", "UnityPatcher~", "UnityPatcher.exe");

                        Process.Start(new ProcessStartInfo
                        {
                            FileName = exePath,
                            Arguments = $"\"{EditorApplication.applicationContentsPath}\" -u \"{Application.dataPath}\"",
                            UseShellExecute = true
                        });

                        EditorApplication.Exit(0);
                    }
                }
            }
            else
            {
                GUILayout.Label($"Unity {Application.unityVersion} is not patched.", _redText);
                GUILayout.Label("If this project uses file-scoped namespaces in MonoBehaviour or ScriptableObject files, they will not be recognised by Unity. To fix this, apply the patch.", _wrappingText);
                if (GUILayout.Button("Patch"))
                {
                    if (EditorUtility.DisplayDialog("Patch Unity?", "Unity needs to be restarted in order for the patch to be applied. Save your work before continuing.", "OK", "Cancel"))
                    {
                        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

                        string packagePath = FileUtil.GetPhysicalPath("Packages/com.alexejhero.unitycsharp11");
                        if (packagePath == "Packages/com.alexejhero.unitycsharp11") packagePath = FileUtil.GetPhysicalPath("Assets");
                        string exePath = Path.Combine(packagePath, "UnityCSharp11", "UnityPatcher~", "UnityPatcher.exe");

                        Process.Start(new ProcessStartInfo
                        {
                            FileName = exePath,
                            Arguments = $"\"{EditorApplication.applicationContentsPath}\" -p \"{Application.dataPath}\"",
                            UseShellExecute = true
                        });

                        EditorApplication.Exit(0);
                    }
                }
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            PackageSettingsProvider provider = new()
            {
                keywords = new string[] {"c#", "11"}
            };

            return provider;
        }
    }
}
