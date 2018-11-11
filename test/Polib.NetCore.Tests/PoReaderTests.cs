using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polib.NetCore.Tests.Properties;
using Polib.Net.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polib.Net;

namespace Polib.NetCore.Tests
{
    [TestClass]
    public class PoReaderTests : TestsBase
    {
        [TestMethod]
        public void Should_Read_PoFile()
        {
            // ARRANGE
            Catalog catalog;

            // ACT
            catalog = PoFileReader.Parse(Resources.admin_fr_FR, CULTURE);

            // ASSERT
            Assert.AreEqual(8, catalog.Headers.Count);
            Assert.AreEqual(2427, catalog.Entries.Count);
        }

        [TestMethod]
        public void Should_Find_All_PO_Files_In_Directory()
        {
            // ARRANGE
            TranslationManager.Instance.PoFilesDirectory = FilesOutputPath;

            // ACT
            var dictionary = PoFileReader.ReadAll(FilesOutputPath, CULTURE
                , skipComments: true
                , includeSubdirectories: true
                , includeRegionCode2: true).GroupByCulture();
            
            // ASSERT

            // only one dictionary should be present because it's the same culture
            Assert.AreEqual(1, dictionary.Count);

            // we expect at least 2 files with the specified culture in the 'Files' directory
            // there should be at least 'admin-fr_FR.po', 'messages-fr-FR.po' (and sometimes 'admin-copied-fr-FR.po')
            Assert.IsTrue(dictionary.GetCatalogs().Count() >= 2);
        }
        
        [TestMethod]
        public async Task Should_Monitor_File_Changes()
        {
            // ARRANGE
            const string COPIED_FILENAME = "admin-copied-fr-FR.po";
            var success = false;
            var counter = 0;
            var manager = TranslationManager.Instance;
            var file = Path.Combine(FilesOutputPath, ADMIN_FILE);
            var fileCopied = Path.Combine(FilesOutputPath, COPIED_FILENAME);

            // first, make sure that the file to be copied doesn't exist; this shouldn't 
            // have an impact as only 'Changed' and 'Created' events are detected
            if (File.Exists(fileCopied)) File.Delete(fileCopied);

            manager.PoFilesDirectory = FilesOutputPath;
            manager.FileWatcher.MonitorFileChanges = true;

            await manager.LoadTranslationsAsync(CULTURE);

            // set up event handler for file change notifications
            manager.FileWatcher.TranslationFilesSynchronized += (s, e) =>
            {
                // compare the file names
                var fileName = e.Catalogs[0].FileName;
                success = string.Equals(fileCopied, fileName, StringComparison.OrdinalIgnoreCase);
            };

            // ACT
            // do some IO operations, say create a copy of the original file
            File.Copy(file, fileCopied);

            // wait for a while before quitting
            while (counter < 30)
            {
                if (success) break;
                await Task.Delay(100);
                counter++;
            }
            
            // ASSERT
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void Should_Find_Catalog_Encoding()
        {
            // ARRANGE
            var file = Path.Combine(FilesOutputPath, ADMIN_FILE);

            // ACT
            PoFileReader.ReadHeader(file).GetCharset(out var charset);
            
            // ASSERT
            Assert.AreEqual("UTF-8", charset);

            var enc = Encoding.GetEncoding(charset);
            Assert.AreEqual(Encoding.UTF8, enc);
        }

        [TestMethod]
        public void Should_Translate_Singular()
        {
            // ARRANGE
            var manager = TranslationManager.Instance;

            manager.TranslationsLoading += (sender, e) =>
            {
                // cancel the file read operations and parse the resource string instead
                e.Cancel = true;
                e.Catalogs = new[] { PoFileReader.Parse(Resources.admin_fr_FR, CULTURE, true) };
            };

            // ACT
            var result = manager.Translate(CULTURE, "MediaElement.js upgraded to 4.2.6");

            // ASSERT
            Assert.AreEqual("Mise à niveau de MediaElement.js en version 4.2.6", result);
        }

        [TestMethod]
        public void Should_Translate_Plural()
        {
            // ARRANGE
            const int FILE_COUNT = 3;
            var expected = string.Format("{0} fichiers médias restaurés depuis la corbeille.", FILE_COUNT);
            var manager = TranslationManager.Instance;
            manager.PoFilesDirectory = FilesOutputPath;

            // ACT
            var result = manager.TranslatePlural(CULTURE
                , "{0} media file restored from the trash."
                , "{0} media files restored from the trash.", FILE_COUNT, FILE_COUNT);

            // ASSERT
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Should_Merge_Files()
        {
            // ARRANGE
            var original = PoFileReader.Read(PoMessagesFile, "fr-FR");
            Catalog merged;

            // ACT
            merged = PoFileReader.Merge(PoMessagesFile, PoTemplateMessagesFile);

            // ASSERT
            Assert.IsTrue(original.Entries.Count < merged.Entries.Count);
        }
        
        //[TestMethod]
        public async Task Run_Caching_BenchMarks()
        {
            var manager = TranslationManager.Instance;
            // preload catalogs
            manager.PoFilesDirectory = FilesOutputPath;
            await manager.LoadTranslationsAsync(CULTURE);

            for (int i = 0; i < MaxIterations.Length; i++)
            {
                RunCachingBenchMark(false, MaxIterations[i]);
                RunCachingBenchMark(true, MaxIterations[i]);
            }
        }

        #region helpers

        static void RunCachingBenchMark(bool disabled, int maxIterations)
        {
            // ARRANGE

            if (maxIterations >= 10000)
            {
                GC.Collect();
                Trace.WriteLine(string.Format("Garbage Collection Status: {0}", GC.WaitForFullGCComplete()));
            }

            var file = Path.Combine(FilesOutputPath, ADMIN_FILE);
            var messages = GetSampleTranslations();
            var watch = new Stopwatch();
            var memStart = Environment.WorkingSet;
            var manager = TranslationManager.Instance;

            // explicitly set caching
            manager.CachingDisabled = disabled;

            // ACT
            watch.Start();

            for (int i = 0; i < maxIterations; i++)
            {
                for (int k = 0; k < messages.Length; k++)
                {
                    manager.Translate(CULTURE, messages[k]);
                }
            }

            watch.Stop();

            var memEnd = Environment.WorkingSet;

            using (var fs = File.OpenWrite(Path.Combine(FilesOutputPath, BENCHMARK_FILE)))
            {
                // go to the end of the file
                fs.Seek(fs.Length, SeekOrigin.Begin);

                fs.WriteLine();
                fs.WriteTime();
                fs.WriteLine("{0:N0} Iterations {1} Caching", maxIterations, disabled ? "Without" : "WITH");
                fs.WriteLine("Initial Memory: {0:N0}", memStart);
                fs.WriteLine("Final Memory: {0:N0}", memEnd);
                fs.WriteLine("Memory Delta: {0:N0}", memEnd - memStart);

                if (watch.ElapsedMilliseconds <= 1L)
                    fs.WriteLine("Performance: < 1 ms");
                else
                    fs.WriteLine("Performance: {0:N0} ms", watch.ElapsedMilliseconds);

                fs.WriteLine();
                fs.WriteLine(DashedLine);
            }

            // ASSERT
            // This is benchmarking, nothing to assert!
        }

        static string[] GetSampleTranslations()
        {
            return new[]
            {
                "Learn how Microsoft's Azure cloud platform allows you to build, deploy, and scale web apps.",

                "This changeset cannot be further modified.",

                "Improvements to Roles and Capabilities",

                "New capabilities have been introduced that allow granular management of plugins and translation files. In addition, the site switching process in multisite has been fine-tuned to update the available roles and capabilities in a more reliable and coherent way.",

                "WordPress 4.9 includes an upgraded version of MediaElement.js, which removes dependencies on jQuery, improves accessibility, modernizes the UI, and fixes many bugs.",

                "MediaElement.js upgraded to 4.2.6",

                "We&#8217;ve introduced a new code editing library, CodeMirror, for use within core. Use it to improve any code writing or editing experiences within your plugins, like CSS or JavaScript include fields.",

                "CodeMirror available for use in your themes and plugins",

                "Sorry, you must be logged in to reply to a comment.",

                "Learn how Microsoft's Azure cloud platform allows you to build, deploy, and scale web apps.",
            };
        }

        #endregion
    }
}
