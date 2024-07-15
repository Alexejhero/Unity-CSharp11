using UnityEditor;
using UnityEngine;

namespace UnityCSharp11
{
    public class CSharp11Window : EditorWindow
    {
        [MenuItem("Tools/C# 11")]
        public static void ShowWindow()
        {
            GetWindow<CSharp11Window>("C# 11");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Enable C# 11"))
            {
                Patch(true);
            }

            if (GUILayout.Button("Disable C# 11"))
            {
                Patch(false);
            }
        }

        private void Patch(bool enable)
        {
        }
    }
}
