using System.IO;
using System.Text;

namespace Polib.Net.IO
{
    /// <summary>
    /// Specifies the data contract required by classes that write catalog changes to a media.
    /// </summary>
    public interface IPoFileWriter
    {
        /// <summary>
        /// Writes the changes back to a temporary file and returns the path pointing at it.
        /// </summary>
        /// <param name="excludeHeaders">Whether to exclude the headers during the operation.</param>
        /// <param name="encoding">The text encoding to use. If null, the catalog's encoding is used.</param>
        /// <returns></returns>
        string SaveChanges(bool excludeHeaders = false, Encoding encoding = null);

        /// <summary>
        /// Writes the changes back to the specified file <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The fully-qualified name, including the path, of the file changes are written to.</param>
        /// <param name="excludeHeaders">Whether to exclude the headers during the operation.</param>
        /// <param name="encoding">The text encoding to use. If null, the catalog's encoding is used.</param>
        void SaveChanges(string path, bool excludeHeaders = false, Encoding encoding = null);

        /// <summary>
        /// Writes the changes back to the specified <paramref name="writer"/>, optionally excluding the headers.
        /// </summary>
        /// <param name="writer">An object used to write the changes in the catalog.</param>
        /// <param name="excludeHeaders">Whether to exclude the headers during the operation.</param>
        void SaveChanges(TextWriter writer, bool excludeHeaders = false);

        /// <summary>
        /// Writes back the changes in this catalog to the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">An object used to write the changes in the catalog.</param>
        /// <param name="excludeHeaders">Whether to exclude the headers during the operation.</param>
        /// <param name="encoding">The text encoding to use. If null, the catalog's encoding is used.</param>
        void SaveChanges(Stream stream, bool excludeHeaders = false, Encoding encoding = null);
    }
}