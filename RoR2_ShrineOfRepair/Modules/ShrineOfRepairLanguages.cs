using R2API;
using System.IO;

namespace ShrineOfRepair.Modules
{
    public class ShrineOfRepairLanguages
    {
        public const string LanguageFileName = "ShrineOfRepair.language";
        public const string LanguageFileFolder = "Languages";

        public void Init()
        {
            LanguageAPI.AddPath(Path.Combine(Path.GetDirectoryName(ShrineOfRepairPlugin.PInfo.Location), LanguageFileFolder, LanguageFileName));
        }

    }
}
