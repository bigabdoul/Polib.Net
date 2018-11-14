using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Polib.Net.Synchronization
{
    /// <summary>
    /// Represents a serializable catalog that has been modified on the client side.
    /// </summary>
    public class UpdatedCatalog : IUpdatedCatalog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatedCatalog"/> class.
        /// </summary>
        public UpdatedCatalog()
        {
        }

        /// <summary>
        /// Gets or sets the unique identifier of the catalog.
        /// </summary>
        public virtual string Id { get; set; }

        /// <summary>
        /// Gets or sets the culture of the catalog.
        /// </summary>
        public virtual CultureInfo Culture { get; set; }

        /// <summary>
        /// Gets or sets the name of file associated with this catalog.
        /// </summary>
        public virtual string FileName { get; set; }

        /// <summary>
        /// Gets or sets the header comments.
        /// </summary>
        public virtual string HeaderComments { get; set; }

        /// <summary>
        /// Gets or sets the plural count.
        /// </summary>
        public virtual int PluralCount { get; set; }

        /// <summary>
        /// Gets or sets the last access time.
        /// </summary>
        public virtual DateTime LastAccessTime { get; set; }

        /// <summary>
        /// Gets or sets the collection of updated translations.
        /// </summary>
        public virtual IList<UpdatedTranslation> Items { get; set; }

        /// <summary>
        /// Gets the unique hashed file identifier.
        /// </summary>
        public virtual string FileId => Id;

        /// <summary>
        /// (Experimental) Merges this catalog with <paramref name="other"/>.
        /// </summary>
        /// <param name="other">The catalog to merge with, which actually contains references to new sources.</param>
        /// <returns>The number of new entries added.</returns>
        public virtual int MergeWith(ICatalog other) => this.Merge(other);

        #region NotImplemented ICatalog methods

        /// <summary>
        /// Returns null. For the dictionary's values, use <see cref="Items"/>.
        /// </summary>
        /// <remarks>Don't throw exceptions; might cause problems during serializations.</remarks>
        public virtual IDictionary<string, ITranslation> Entries => null;

        /// <summary>
        /// Returns null.
        /// </summary>
        /// <remarks>Don't throw exceptions; might cause problems during serializations.</remarks>
        public virtual IDictionary<string, string> Headers => null;

        /* Methods are not supposed to be invoked during serializations, 
         so it's safe to throw exceptions since these methods are not implemented.*/

        /// <summary>
        /// Throws <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool GetCharset(out string value) => throw new NotImplementedException();

        /// <summary>
        /// Throws <see cref="NotImplementedException"/>.
        /// </summary>
        /// <returns></returns>
        public virtual Encoding GetEncoding() => throw new NotImplementedException();

        /// <summary>
        /// Throws <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool GetHeader(string name, out string value) => throw new NotImplementedException();

        #endregion
    }
}
