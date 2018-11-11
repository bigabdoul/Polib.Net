using Microsoft.AspNetCore.Html;
using Polib.Net;
using System.Threading;

namespace Polib.NetCore.Mvc
{
    /// <summary>
    /// Represents the base class for injecting .po files translation capabilities into razor views.
    /// </summary>
    public class RazorPageTranslationManager : IRazorPageTranslationManager
    {
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RazorPageTranslationManager"/> class.
        /// </summary>
        public RazorPageTranslationManager()
        {
        }
        
        #endregion

        #region public static properties

        /// <summary>
        /// Returns a reference to the <see cref="TranslationManager"/> class instance.
        /// </summary>
        public static TranslationManager TranslationManager { get; } = TranslationManager.Instance;

        #endregion

        #region public methods

        #region versions returning IHtmlContent
        
        /// <summary>
        /// Returns a localized HTML-encoded string from a 
        /// translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="singular">The identifier of the localized string.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public virtual IHtmlContent H(string singular, params object[] formatArgs)
            => new HtmlString(T(singular, formatArgs));

        /// <summary>
        /// Returns a contextual localized HTML-encoded string from 
        /// a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="context">The context to use.</param>
        /// <param name="singular">The identifier of the singular form of the translation.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public virtual IHtmlContent Hx(string context, string singular, params object[] formatArgs)
            => new HtmlString(Tx(context, singular, formatArgs));

        /// <summary>
        /// Returns the singular or plural form of a localized HTML-encoded 
        /// string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="singular">The identifier of the singular form of the translation.</param>
        /// <param name="plural">The identifier of the plural form of the translation.</param>
        /// <param name="number">A number that determines the singular or plural form to choose from.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public virtual IHtmlContent Hn(string singular, string plural, long number, params object[] formatArgs)
            => new HtmlString(Tn(singular, plural, number, formatArgs));

        /// <summary>
        /// Returns the singular or plural form of a contextual localized HTML-encoded 
        /// string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="context">The context to use.</param>
        /// <param name="singular">The identifier of the singular form of the translation.</param>
        /// <param name="plural">The identifier of the plural form of the translation.</param>
        /// <param name="count">A number that determines the singular or plural form to choose from.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public virtual IHtmlContent Hnx(string context, string singular, string plural, long count, params object[] formatArgs)
            => new HtmlString(Tnx(context, singular, plural, count, formatArgs));

        #endregion

        #region equivalent versions returning string
        
        /// <summary>
        /// Returns a localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="singular">The identifier of the localized string.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public virtual string T(string singular, params object[] formatArgs)
            => TranslationManager.Translate(Culture, null, singular, formatArgs);

        /// <summary>
        /// Returns a contextual localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="singular">The identifier of the localized string.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public virtual string Tx(string context, string singular, params object[] formatArgs)
            => TranslationManager.Translate(Culture, context, singular, formatArgs);

        /// <summary>
        /// Returns the singular or plural form of a localized string from a translated .po file.
        /// </summary>
        /// <param name="singular">The identifier of the singular form of the translation.</param>
        /// <param name="plural">The identifier of the plural form of the translation.</param>
        /// <param name="count">A number that determines the singular or plural form to choose from.</param>
        /// <param name="formatArgs"></param>
        /// <returns></returns>
        public virtual string Tn(string singular, string plural, long count, params object[] formatArgs)
            => TranslationManager.TranslatePlural(Culture, null, singular, plural, count, formatArgs);

        /// <summary>
        /// Returns the singular or plural form of a contextual localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="singular">The identifier of the singular form of the translation.</param>
        /// <param name="plural">The identifier of the plural form of the translation.</param>
        /// <param name="count">A number that determines the singular or plural form to choose from.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public virtual string Tnx(string context, string singular, string plural, long count, params object[] formatArgs)
            => TranslationManager.TranslatePlural(Culture, context, singular, plural, count, formatArgs);

        #endregion

        #endregion

        #region private helpers

        /// <summary>
        /// Gets the current UI culture info name of the current thread.
        /// </summary>
        string Culture
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name;
            }
        }
        
        #endregion
    }
}
