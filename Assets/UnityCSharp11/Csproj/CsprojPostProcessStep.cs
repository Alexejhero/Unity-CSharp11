using System.Xml.Linq;

namespace UnityCSharp11.Csproj
{
    public abstract class CsprojPostProcessStep
    {
        /// <summary>
        /// Run any initialization that needs to happen on the main thread.<br/>
        /// Hot Reload will call <see cref="PostProcess"/> from the thread pool.
        /// </summary>
        public abstract void Initialize();

        /// <summary>Process the csproj XML.</summary>
        /// <param name="doc">Current XML representation. May have already been modified by previous steps.</param>
        /// <param name="path">Path to the csproj (relative to the project directory).</param>
        public abstract void PostProcess(XDocument doc, string path);
    }
}
