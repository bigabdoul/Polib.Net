using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Polib.Net
{
    /// <summary>
    /// Specifies the data contract required by classes that represent a catalog of translated items.
    /// </summary>
    public interface ICatalog
    {
        /// <summary>
        /// Gets or sets the first comments before the <see cref="Headers"/> collection.
        /// </summary>
        string HeaderComments { get; set; }

        /// <summary>
        /// Gets the headers of this reader.
        /// </summary>
        IDictionary<string, string> Headers { get; }

        /// <summary>
        /// Gets the culture of the elements contained in <see cref="Entries"/>.
        /// </summary>
        CultureInfo Culture { get; set; }

        /// <summary>
        ///  Gets the list of translation entries.
        /// </summary>
        IDictionary<string, ITranslation> Entries { get; }

        /// <summary>
        /// Gets the unique file identifier of the current catalog.
        /// </summary>
        string FileId { get; }

        /// <summary>
        /// Gets or sets the name of the file used during the read operation.
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// Gets or sets the date the file was last read.
        /// </summary>
        DateTime LastAccessTime { get; set; }

        /// <summary>
        /// Gets or sets the number of plural forms.
        /// </summary>
        int PluralCount { get; set; }

        /// <summary>
        /// Attempts to return the value of the specified header name.
        /// </summary>
        /// <param name="name">The name of the header to return.</param>
        /// <param name="value">Returns the value of the header, if found.</param>
        /// <returns></returns>
        bool GetHeader(string name, out string value);

        /// <summary>
        /// Attempts to determine the character set defined in the 'Content-Type' header.
        /// </summary>
        /// <param name="value">Returns the character set, if present.</param>
        /// <returns></returns>
        bool GetCharset(out string value);

        /// <summary>
        /// Finds the character set and returns a new instance of the corresponding <see cref="Encoding"/> class.
        /// </summary>
        /// <returns>An initialized instance of the <see cref="Encoding"/> class, or null.</returns>
        Encoding GetEncoding();

        /// <summary>
        /// Merges this catalog with <paramref name="other"/>.
        /// </summary>
        /// <param name="other">The catalog to merge with, which actually contains references to new sources.</param>
        /// <returns>The number of new entries added.</returns>
        int MergeWith(ICatalog other);
    }
}