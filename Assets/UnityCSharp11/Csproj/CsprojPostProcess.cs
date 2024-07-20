using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityCSharp11.Csproj
{
    public sealed partial class CsprojPostProcess : AssetPostprocessor
    {
        /// <summary>
        /// Hook to determine whether or not to run initialization ourselves.<br/>
        /// Intended for use with Hot Reload, which performs initialization separate from Unity's <see cref="InitializeOnLoadMethodAttribute"/>.<br/>
        /// </summary>
        /// <remarks>Implementations must have a parameterless constructor.</remarks>
        private interface IInitHook
        {
            /// <summary><inheritdoc cref="IInitHook"/></summary>
            /// <returns>
            /// <see langword="true"/> to proceed with default initialization, <see langword="false"/> to skip it.<br/>
            /// Note: All hooks will be called, if any return <see langword="false"/> the initialization will be skipped.
            /// </returns>
            bool Hook();
        }
        private static readonly List<CsprojPostProcessStep> _pipeline = new();
        
        private static readonly Assembly _assembly = typeof(CsprojPostProcess).Assembly;
        static CsprojPostProcess()
        {
            _pipeline.AddRange(_assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(CsprojPostProcessStep)))
                .Select(Activator.CreateInstance)
                .Cast<CsprojPostProcessStep>());
        }

        private static readonly string _libPackagesPath = Path.Join("Library","Packages");
        public static string OnGeneratedCSProject(string path, string content)
        {
            string relativePath = Path.GetRelativePath(Environment.CurrentDirectory, path);
            // don't touch packages, who knows what they did to themselves
            if (relativePath.StartsWith(_libPackagesPath)) return content;

            XDocument doc = XDocument.Parse(content);
            doc.Root!.Name = doc.Root.Name.LocalName;
            foreach (XElement elem in doc.Descendants())
            {
                elem.Name = elem.Name.LocalName; // take the xmlns out back
                elem.ReplaceAttributes(elem.Attributes()
                    .Where(attr => !attr.IsNamespaceDeclaration)
                    .Select(attr => new XAttribute(attr.Name.LocalName, attr.Value)));
            }

            foreach (CsprojPostProcessStep step in _pipeline)
            {
                try
                {
                    step.PostProcess(doc, path);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    throw;
                }
            }

            return doc.ToString();
        }

        [InitializeOnLoadMethod]
        public static void InitializeOnLoad()
        {
            bool run = _assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(IInitHook)))
                .Select(Activator.CreateInstance)
                .Cast<IInitHook>()
                .Aggregate(true, (curr, hook) => hook.Hook() && curr); // keep short circuiting in mind
            if (run) Initialize();
        }

        private static void Initialize()
        {
            foreach (CsprojPostProcessStep step in _pipeline)
            {
                step.Initialize();
            }
        }
    }
}
