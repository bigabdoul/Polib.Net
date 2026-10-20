using System.Collections.Generic;

namespace Polib.Net
{
    /// <summary>
    /// Specifies the data contract required by classes to implement a translation entry.
    /// </summary>
    public interface ITranslation
    {
        /// <summary>
        /// Gets or sets the msgctxt property.
        /// </summary>
        string Context { get; set; }

        /// <summary>
        /// Gets or sets the extracted comments.
        /// </summary>
        string ExtractedComments { get; set; }

        /// <summary>
        /// Gets a collection of comment flags.
        /// </summary>
        IList<string> Flags { get; }

        /// <summary>
        /// Determines whether this translation has a plural form.
        /// </summary>
        bool IsPlural { get; }

        /// <summary>
        /// Gets or sets the msgid_plural property.
        /// </summary>
        string Plural { get; set; }

        /// <summary>
        /// Gets a collection of commented references for this translation.
        /// </summary>
        IList<string> References { get; }

        /// <summary>
        /// Gets or sets the msgid property.
        /// </summary>
        string Singular { get; set; }

        /// <summary>
        /// Gets a collection of msgstr properties.
        /// </summary>
        IList<string> Translations { get; }

        /// <summary>
        /// Gets or sets the translator comments.
        /// </summary>
        string TranslatorComments { get; set; }

        /// <summary>
        /// Generates a unique key for this entry.
        /// </summary>
        /// <returns>The key or null if the entry is empty.</returns>
        string Key { get; }

        /// <summary>
        /// Returns the plural form of this translation.
        /// </summary>
        /// <param name="count">The number for which to evaluate the plural form.</param>
        /// <returns></returns>
        string GetPlural(long count);

        /// <summary>
        /// Returns the plural form of this translation.
        /// </summary>
        /// <param name="count">The number for which to evaluate the plural form.</param>
        /// <param name="fallback">
        /// The culture to use if either the current translation's catalog or the catalog's plural evaluator is null.
        /// </param>
        /// <returns></returns>
        string GetPlural(long count, System.Globalization.CultureInfo fallback);

        ///// <summary>
        ///// Merges this <see cref="ITranslation"/> with <paramref name="other"/>.
        ///// </summary>
        ///// <param name="other">The other <see cref="ITranslation"/> to merge into this instance.</param>
        //void MergeWith(ITranslation other);
    }
}