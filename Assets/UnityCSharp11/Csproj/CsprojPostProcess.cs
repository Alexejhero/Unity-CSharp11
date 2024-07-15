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
    public partial class CsprojPostProcess : AssetPostprocessor
    {
        private static readonly List<CsprojPostProcessStep> _pipeline = new();

        static CsprojPostProcess()
        {
            Assembly assembly = typeof(CsprojPostProcess).Assembly;
            _pipeline.AddRange(assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(CsprojPostProcessStep)))
                .Select(Activator.CreateInstance)
                .Cast<CsprojPostProcessStep>());
        }

        public static string OnGeneratedCSProject(string path, string content)
        {
            string relativePath = Path.GetRelativePath(Environment.CurrentDirectory, path);
            if (relativePath.StartsWith("Library")) return content;

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

        public string OnGeneratedCSProjectThreaded(string path, string contents)
        {
            return OnGeneratedCSProject(path, contents);
        }

        [InitializeOnLoadMethod]
        public static void InitializeOnMainThreadNoHotReload()
        {
            if (typeof(CsprojPostProcess).GetInterface("IHotReloadProjectGenerationPostProcessor") != null)
            {
                // initialization is handled by Hot Reload
                return;
            }

            foreach (CsprojPostProcessStep step in _pipeline)
            {
                step.Initialize();
            }
        }

        public void InitializeOnMainThread()
        {
            foreach (CsprojPostProcessStep step in _pipeline)
            {
                step.Initialize();
            }
        }

        #region Hot Reload interface impl

        public int CallbackOrder => 0;

        public void OnGeneratedCSProjectFilesThreaded()
        {
        }

        public string OnGeneratedSlnSolutionThreaded(string path, string contents) => contents;

        #endregion Hot Reload interface impl
    }
}
