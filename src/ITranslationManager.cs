using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polib.Net.IO;

namespace Polib.Net
{
    /// <summary>
    /// Specifies the contract required for classes used to manage translation files.
    /// </summary>
    public interface ITranslationManager
    {
        /// <summary>
        /// Determines whether to disable caching of translations.
        /// </summary>
        bool CachingDisabled { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of <see cref="ICatalog"/> list grouped by culture containing all read catalogs.
        /// </summary>
        IDictionary<string, IList<ICatalog>> Catalogs { get; }

        /// <summary>
        /// Gets an object that watches translation file changes.
        /// </summary>
        ITranslationFileWatcher FileWatcher { get; }

        /// <summary>
        /// Determines whether to include the regioncode2 for a specific culture when searching .po files. The default value is true.
        /// </summary>
        bool IncludeCultureRegionCode2 { get; set; }

        /// <summary>
        /// Gets or sets the .po files directory path.
        /// </summary>
        string PoFilesDirectory { get; set; }

        /// <summary>
        /// Determines whether to skip comments when reading .po files. The default value is true.
        /// </summary>
        bool SkipComments { get; set; }

        /// <summary>
        /// Event fired before translations are loaded.
        /// </summary>
        event EventHandler<TranslationEventArgs> TranslationsLoading;

        /// <summary>
        /// Clears the translation cache.
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Ensures that translations for the specified culture are loaded asynchronously.
        /// </summary>
        /// <param name="culture">The culture or domain name of the translation to use.</param>
        /// <returns></returns>
        Task LoadTranslationsAsync(string culture);

        /// <summary>
        /// Invalidates any previously loaded translation files.
        /// </summary>
        void ResetTranslations();

        /// <summary>
        /// Returns a localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="culture">The culture or domain name of the translation to use.</param>
        /// <param name="singular">The identifier of the localized string.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        string Translate(string culture, string singular, params object[] formatArgs);

        /// <summary>
        /// Returns a contextual localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="culture">The culture or domain name of the translation to use.</param>
        /// <param name="context">The message context.</param>
        /// <param name="singular">The identifier of the localized string.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        string Translate(string culture, string context, string singular, params object[] formatArgs);

        /// <summary>
        /// Returns the singular or plural form of a localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="culture">The culture or domain name of the translation to use.</param>
        /// <param name="singular">The identifier of the singular form of the translation.</param>
        /// <param name="plural">The identifier of the plural form of the translation.</param>
        /// <param name="count">A number that determines the singular or plural form to choose from.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        string TranslatePlural(string culture, string singular, string plural, long count, params object[] formatArgs);

        /// <summary>
        /// Returns the singular or plural form of a contextual localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="culture">The culture or domain name of the translation to use.</param>
        /// <param name="context">The message context.</param>
        /// <param name="singular">The identifier of the singular form of the translation.</param>
        /// <param name="plural">The identifier of the plural form of the translation.</param>
        /// <param name="count">A number that determines the singular or plural form to choose from.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        string TranslatePlural(string culture, string context, string singular, string plural, long count, params object[] formatArgs);
    }
}