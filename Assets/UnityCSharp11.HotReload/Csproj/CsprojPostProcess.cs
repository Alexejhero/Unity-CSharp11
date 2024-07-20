using JetBrains.Annotations;
using SingularityGroup.HotReload.Editor.ProjectGeneration;

// ReSharper disable once CheckNamespace
namespace UnityCSharp11.Csproj
{
    partial class CsprojPostProcess : IHotReloadProjectGenerationPostProcessor
    {
        public int CallbackOrder => 0;
        
        public void InitializeOnMainThread() => Initialize();
        public string OnGeneratedSlnSolutionThreaded(string path, string contents) => contents;
        public void OnGeneratedCSProjectFilesThreaded() { }
        public string OnGeneratedCSProjectThreaded(string path, string contents) => OnGeneratedCSProject(path, contents);
        [UsedImplicitly]
        private struct HotReloadInitHook : IInitHook
        {
            public bool Hook() => false;
        }
    }

}
