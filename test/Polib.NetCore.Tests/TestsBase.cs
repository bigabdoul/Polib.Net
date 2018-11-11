using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Polib.NetCore.Tests
{
    public class TestsBase
    {
        #region constants & statics

        protected internal const string CULTURE = "fr-FR";
        protected internal const string ADMIN_FILE = "admin-fr_FR.po";
        protected internal const string BENCHMARK_FILE = "benchmarks.txt";

        protected internal static string GetAssemblyLocation(params string[] subPaths)
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (subPaths?.Length > 0)
            {
                var list = new List<string> { dir };
                list.AddRange(subPaths);
                return Path.Combine(list.ToArray());
            }

            return dir; 
        }

        protected internal static string FilesOutputPath = GetAssemblyLocation("Files");
        protected internal static string PoMessagesFile = Path.Combine(FilesOutputPath, "messages-fr-FR.po");
        protected internal static string PoTemplateMessagesFile = Path.Combine(FilesOutputPath, "messages.pot");

        protected internal static readonly string DashedLine = new string('-', 60);
        protected internal static readonly int[] MaxIterations = new[] { 100, 1000, 10000, 100000, 1000000, 10000000 };

        #endregion
    }
}
