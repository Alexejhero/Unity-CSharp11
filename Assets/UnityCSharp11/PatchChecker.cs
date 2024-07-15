using System.IO;
using System.Linq;
using Mono.Cecil;
using UnityEditor;

namespace UnityCSharp11
{
    public static class PatchChecker
    {
        public static bool IsPatched()
        {
            string path = Path.Combine(EditorApplication.applicationContentsPath, "Tools", "Unity.SourceGenerators", "Unity.SourceGenerators.dll");
            using AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(path);
            return asm.MainModule.GetTypes().Any(t => t.Name == "CSharp11NamespacePatch");
        }
    }
}
