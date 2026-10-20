using Polib.Net.Plurals;
using System.Collections.Generic;
using System.Linq;

namespace Polib.Net
{
    /// <summary>
    /// Represents a translation entry in a .po file.
    /// </summary>
    public class Translation : ITranslation
    {
        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Translation"/> class.
        /// </summary>
        public Translation()
        {
            Translations = new List<string>();
            References = new List<string>();
            Flags = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Translation"/> class using the specified parameter.
        /// </summary>
        /// <param name="parent">The catalog to which this translation belongs.</param>
        public Translation(Catalog parent) : this()
        {
            Catalog = parent;
        }
        
        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the msgctxt property.
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        /// Gets or sets the msgid property.
        /// </summary>
        public string Singular { get; set; }

        /// <summary>
        /// Gets or sets the msgid_plural property.
        /// </summary>
        public string Plural { get; set; }

        /// <summary>
        /// Gets a collection of msgstr properties.
        /// </summary>
        public virtual IList<string> Translations { get; }

        /// <summary>
        /// Gets a collection of commented references for this translation.
        /// </summary>
        public virtual IList<string> References { get; }

        /// <summary>
        /// Gets or sets the extracted comments.
        /// </summary>
        public string ExtractedComments { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the translator comments.
        /// </summary>
        public string TranslatorComments { get; set; } = string.Empty;

        /// <summary>
        /// Gets a collection of comment flags.
        /// </summary>
        public virtual IList<string> Flags { get; }

        /// <summary>
        /// Determines whether this translation has a plural form.
        /// </summary>
        public bool IsPlural { get => Translations.Count > 1; }

        /// <summary>
        /// Gets the catalog to which the current translation belongs.
        /// </summary>
        public Catalog Catalog { get; }

        /// <summary>
        /// Generates a unique key for this entry.
        /// </summary>
        /// <returns>The key or null if the entry is empty.</returns>
        public virtual string Key
        {
            get
            {
                if (string.IsNullOrEmpty(Singular))
                {
                    return null;
                }

                // Prepend context and EOT, like in MO files
                var key = string.IsNullOrEmpty(Context) ? Singular : (Context + char.ConvertFromUtf32(4) + Singular);
                // Standardize on \n line endings
                return key.Replace("\r\n", "\n").Replace('\r', '\n');
            }
        }

        #endregion

        #region public instance methods

        /// <summary>
        /// Returns the singular form of this translation, or the message identifier.
        /// </summary>
        /// <returns></returns>
        public virtual string GetSingular()
        {
            if (Translations.Count > 0)
            {
                return Translations[0];
            }
            return Singular;
        }

        /// <summary>
        /// Returns the plural form of this translation.
        /// </summary>
        /// <param name="count">The number for which to evaluate the plural form.</param>
        /// <returns></returns>
        public virtual string GetPlural(long count) => GetPlural(count, null);

        /// <summary>
        /// Returns the plural form of this translation.
        /// </summary>
        /// <param name="count">The number for which to evaluate the plural form.</param>
        /// <param name="fallbackCulture">
        /// The culture to use if either <see cref="Catalog"/> or <see cref="Catalog.PluralEvaluator"/> is null.
        /// </param>
        /// <returns></returns>
        public virtual string GetPlural(long count, System.Globalization.CultureInfo fallbackCulture)
        {
            var index = 0;
            var evaluator = Catalog?.PluralEvaluator;

            if (evaluator != null)
            {
                index = evaluator.Evaluate(count);
            }
            else if (fallbackCulture != null)
            {
                index = PluralFormsEvaluator.GetPluralIndex(fallbackCulture, count);
            }
            else
            {
                // default
                index = PluralFormsEvaluator.Default.Evaluate(count);
            }

            if (index < 0 || index >= Translations.Count)
                throw new System.ArgumentOutOfRangeException("index"
                    , $"Plural forms index must fall between 0 and {Translations.Count - 1}");

            return Translations[index];
        }

        /// <summary>
        /// Merges this <see cref="ITranslation"/> with <paramref name="other"/>.
        /// </summary>
        /// <param name="other">The other <see cref="ITranslation"/> to merge into this instance.</param>
        public virtual void MergeWith(ITranslation other)
        {
            MergeDistinct(other.Flags, Flags);
            MergeDistinct(other.References, References);
            MergeDistinct(other.Translations, Translations);

            if (!string.Equals(ExtractedComments, other.ExtractedComments))
            {
                ExtractedComments += other.ExtractedComments;
            }
        }
        
        /// <summary>
        /// Returns the string representation of the current instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => GetSingular();

        #endregion

        #region static helpers

        internal static IList<T> Merge<T>(IEnumerable<T> array1, IEnumerable<T> array2)
        {
            var list = new List<T>(array1);
            list.AddRange(array2);
            return list;
        }

        internal static void MergeDistinct<T>(IEnumerable<T> source, IList<T> destination)
        {
            var collection = Merge(source, destination).Distinct().ToArray();
            destination.Clear();
            foreach (var item in collection)
            {
                destination.Add(item);
            }
        } 

        #endregion
    }
}
