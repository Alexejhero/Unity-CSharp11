using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace UnityCSharp11.Csproj
{
    [UsedImplicitly]
    public sealed class UpgradeLangVersion : CsprojPostProcessStep
    {
        public override void Initialize()
        {
            CompilerArguments.SetCompilerArgument("langversion", "preview");
        }

        public override void PostProcess(XDocument doc, string path)
        {
            XElement langVer = doc.Root!
                .Elements("PropertyGroup")
                .Select(group => group.Element("LangVersion"))
                .FirstOrDefault(e => e is not null);
            if (langVer is null)
            {
                XElement grp = new("PropertyGroup");
                langVer = new XElement("LangVersion");
                grp.Add(langVer);
                doc.Root.Add(grp);
            }

            // C#11 is the most recent supported by Unity 2022 (/langversion:preview)
            langVer.Value = "11"; // csproj is only for IDEs, which now in 2024 interpret "preview" as 12/13/beyond
        }
    }
}
