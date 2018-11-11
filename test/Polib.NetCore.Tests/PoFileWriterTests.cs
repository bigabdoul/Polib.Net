using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polib.Net;
using Polib.Net.IO;
using Polib.NetCore.Tests.Properties;
using System;
using System.Diagnostics;

namespace Polib.NetCore.Tests
{
    [TestClass]
    public class PoFileWriterTests : TestsBase
    {
        #region constants

        const string TEXT = "New capabilities have been introduced that allow granular management of plugins and translation files. In addition, the site switching process in multisite has been fine-tuned to update the available roles and capabilities in a more reliable and coherent way.";

        private const string LoremIpsum = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas varius sapien vel purus hendrerit vehicula. Integer hendrerit viverra turpis, ac sagittis arcu pharetra id. Sed dapibus enim non dui posuere sit amet rhoncus tellus
consectetur. Proin blandit lacus vitae nibh tincidunt cursus. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Nam tincidunt purus at tortor tincidunt et aliquam dui gravida. Nulla consectetur sem vel felis vulputate et imperdiet orci pharetra. Nam vel tortor nisi. Sed eget porta tortor. Aliquam suscipit lacus vel odio faucibus tempor. Sed ipsum est, condimentum eget eleifend ac, ultricies non dui. Integer tempus, nunc sed venenatis feugiat, augue orci pellentesque risus, nec pretium lacus enim eu nibh.";

        #endregion

        [TestMethod]
        public void Should_Write_PO_File()
        {
            // ARRANGE
            var catalog = PoFileReader.Parse(Resources.admin_fr_FR, CULTURE);

            // ACT
            var output = new PoFileWriter(catalog).Export();

            // ASSERT
            Assert.IsFalse(string.IsNullOrEmpty(output));

            Trace.WriteLine(output);
        }

        [TestMethod]
        public void Should_Wrap_Words()
        {
            // ARRANGE
            
            // ACT
            var wraps = TEXT.Wordwrap().Trim();
            var paragraphs = wraps.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            // ASSERT
            Assert.AreEqual(4, paragraphs.Length);
            Assert.AreEqual(TEXT.Length, wraps.Length - paragraphs.Length + 1);
        }

        [TestMethod]
        public void Should_Wrap_Words_Regex()
        {
            // ARRANGE

            // ACT
            var wraps = TEXT.WordwrapRx(cut: true).Trim();

            // ASSERT
            var paragraphs = wraps.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(4, paragraphs.Length);
            Assert.AreEqual(TEXT.Length, wraps.Length);
        }

        [TestMethod]
        public void Should_Wrap_Words_CodeProject()
        {
            // ARRANGE

            // ACT
            var wraps = TEXT.WordwrapUncut().Trim();

            // ASSERT
            var paragraphs = wraps.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(4, paragraphs.Length);
            Assert.AreEqual(TEXT.Length, wraps.Length);
        }

        [TestMethod]
        public void Should_Wrap_Text()
        {
            // ARRANGE

            // ACT
            var wraps = TEXT.WrapText().Trim();

            // ASSERT
            var paragraphs = wraps.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(4, paragraphs.Length);
            Assert.AreEqual(TEXT.Length, wraps.Length - paragraphs.Length + 1);
        }
    }
}
