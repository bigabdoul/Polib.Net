using System.Collections.Generic;

namespace Polib.Net.Synchronization
{
    /// <summary>
    /// Represents a serializable object used to update (from a client application) an existing translation.
    /// This is essentially a version of the <see cref="Translation"/> class which has read-write properties.
    /// </summary>
    public class UpdatedTranslation : IUpdatedTranslation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatedTranslation"/> class.
        /// </summary>
        public UpdatedTranslation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatedTranslation"/> class using the specified parameter.
        /// </summary>
        /// <param name="row">The ordinal row number.</param>
        public UpdatedTranslation(int row)
        {
            Row = row;
        }

        /// <summary>
        /// Gets or sets the ordinal row number.
        /// </summary>
        public int Row { get; }

        /// <summary>
        /// Gets or sets the unique key of this translation.
        /// </summary>
        public virtual string UniqueKey { get; set; }

        /// <summary>
        /// Gets the unique of this translation.
        /// </summary>
        public virtual string Key { get => UniqueKey; }

        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        public virtual string Context { get; set; }

        /// <summary>
        /// Gets or sets the singular form.
        /// </summary>
        public virtual string Singular { get; set; }

        /// <summary>
        /// Gets or sets the plural form.
        /// </summary>
        public string Plural { get; set; }

        /// <summary>
        /// Gets or sets the translations collection.
        /// </summary>
        public virtual IList<string> Translations { get; set; }

        /// <summary>
        /// Gets or sets the extracted comments.
        /// </summary>
        public virtual string ExtractedComments { get; set; }

        /// <summary>
        /// Gets or sets the flags collection.
        /// </summary>
        public virtual IList<string> Flags { get; set; }

        /// <summary>
        /// Gets or sets the references collection.
        /// </summary>
        public virtual IList<string> References { get; set; }

        /// <summary>
        /// Gets or sets the translators comments.
        /// </summary>
        public virtual string TranslatorComments { get; set; }
    }
}
