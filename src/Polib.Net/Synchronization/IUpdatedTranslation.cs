using System.Collections.Generic;

namespace Polib.Net.Synchronization
{
    /// <summary>
    /// Specifies the data contract required by classes that implement updatable translations.
    /// </summary>
    public interface IUpdatedTranslation
    {
        /// <summary>
        /// Gets the ordinal row number.
        /// </summary>
        int Row { get; }

        /// <summary>
        /// Gets or sets the unique key of this translation.
        /// </summary>
        string UniqueKey { get; set; }

        /// <summary>
        /// Gets the unique of this translation.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        string Context { get; set; }

        /// <summary>
        /// Gets or sets the singular form.
        /// </summary>
        string Singular { get; set; }

        /// <summary>
        /// Gets or sets the plural form.
        /// </summary>
        string Plural { get; set; }

        /// <summary>
        /// Gets or sets the translations collection.
        /// </summary>
        IList<string> Translations { get; set; }

        /// <summary>
        /// Gets or sets the extracted comments.
        /// </summary>
        string ExtractedComments { get; set; }

        /// <summary>
        /// Gets or sets the flags collection.
        /// </summary>
        IList<string> Flags { get; set; }

        /// <summary>
        /// Gets or sets the references collection.
        /// </summary>
        IList<string> References { get; set; }

        /// <summary>
        /// Gets or sets the translators comments.
        /// </summary>
        string TranslatorComments { get; set; }
    }
}
