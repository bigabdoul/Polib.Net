using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Threading;

namespace Polib.Net.Core.Mvc
{
    /// <summary>
    /// Represents the base class for providing .po files translation capabilities to razor views.
    /// </summary>
    public abstract class PoTranslationRazorPage : RazorPage<object>, IRazorPagePoTranslationManager
    {        
        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PoTranslationRazorPage"/> class.
        /// </summary>
        protected PoTranslationRazorPage()
        {
        }

        #endregion

        #region public static properties

        /// <summary>
        /// Returns a reference to the default instance of the <see cref="PoTranslationManager"/> class.
        /// </summary>
        public static PoTranslationManager TranslationManager { get; } = PoTranslationManager.Instance;

        #endregion

        #region public methods

        #region versions returning IHtmlContent

        /// <summary>
        /// Returns a localized string from a translated .po file.
        /// </summary>
        /// <param name="id">The identifier of the localized string.</param>
        /// <returns></returns>
        public virtual IHtmlContent __(string id) => __(id, null);

        /// <summary>
        /// Returns a localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="id">The identifier of the localized string.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public virtual IHtmlContent __(string id, params object[] formatArgs)
            => new HtmlString(TranslationManager.Translate(id, CultureName, formatArgs));

        /// <summary>
        /// Returns the singular or plural form of a localized string from a translated .po file.
        /// </summary>
        /// <param name="id">The identifier of the singular form of the translation.</param>
        /// <param name="idPlural">The identifier of the plural form of the translation.</param>
        /// <param name="number">A number that determines the singular or plural form to choose from.</param>
        /// <returns></returns>
        public virtual IHtmlContent _n(string id, string idPlural, long number)
            => _n(id, idPlural, number, null);

        /// <summary>
        /// Returns the singular or plural form of a localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="id">The identifier of the singular form of the translation.</param>
        /// <param name="idPlural">The identifier of the plural form of the translation.</param>
        /// <param name="number">A number that determines the singular or plural form to choose from.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public virtual IHtmlContent _n(string id, string idPlural, long number, params object[] formatArgs)
            => new HtmlString(TranslationManager.Translate(id, idPlural, number, CultureName, formatArgs));

        #endregion

        #region equivalent versions returning string

        /// <summary>
        /// Returns a localized string from a translated .po file.
        /// </summary>
        /// <param name="id">The identifier of the localized string.</param>
        /// <returns></returns>
        public virtual string _s(string id) => _s(id, null);

        /// <summary>
        /// Returns a localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="id">The identifier of the localized string.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public virtual string _s(string id, params object[] formatArgs)
            => TranslationManager.Translate(id, CultureName, formatArgs);


        /// <summary>
        /// Returns the singular or plural form of a localized string from a translated .po file.
        /// </summary>
        /// <param name="id">The identifier of the singular form of the translation.</param>
        /// <param name="idPlural">The identifier of the plural form of the translation.</param>
        /// <param name="number">A number that determines the singular or plural form to choose from.</param>
        /// <returns></returns>
        public virtual string _sn(string id, string idPlural, long number)
            => _sn(id, idPlural, number, null);

        /// <summary>
        /// Returns the singular or plural form of a localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="id">The identifier of the singular form of the translation.</param>
        /// <param name="idPlural">The identifier of the plural form of the translation.</param>
        /// <param name="number">A number that determines the singular or plural form to choose from.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public virtual string _sn(string id, string idPlural, long number, params object[] formatArgs)
            => TranslationManager.Translate(id, idPlural, number, CultureName, formatArgs);

        #endregion

        /// <summary>
        /// Invalidates any previously loaded translation files.
        /// </summary>
        public virtual void ResetTranslations() => TranslationManager.ResetTranslations();

        #endregion

        #region private helpers

        /// <summary>
        /// Gets the current UI culture info name of the current thread.
        /// </summary>
        string CultureName { get => Thread.CurrentThread.CurrentUICulture.Name; }
        
        #endregion
    }
}
