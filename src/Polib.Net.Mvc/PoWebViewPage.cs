using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Polib.Net.Mvc
{
    /// <summary>
    /// Represents the base class for a translation-aware razor view.
    /// </summary>
    public abstract class PoWebViewPage : WebViewPage
    {
        #region public static properties

        /// <summary>
        /// Returns a reference to the <see cref="Net.TranslationManager"/> class instance.
        /// </summary>
        public static TranslationManager TranslationManager { get; } = TranslationManager.Instance;

        #endregion

        #region public methods

        #region versions returning IHtmlString

        /// <summary>
        /// Returns a localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="singular">The identifier of the localized string.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public virtual IHtmlString H(string singular, params object[] formatArgs)
            => MvcHtmlString.Create(TranslationManager.Translate(Culture, singular, formatArgs));

        /// <summary>
        /// Returns a contextual localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="context">The context to use.</param>
        /// <param name="singular">The identifier of the singular form of the translation.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public virtual IHtmlString Hx(string context, string singular, params object[] formatArgs)
            => MvcHtmlString.Create(Tx(context, singular, formatArgs));

        /// <summary>
        /// Returns the singular or plural form of a localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="singular">The identifier of the singular form of the translation.</param>
        /// <param name="plural">The identifier of the plural form of the translation.</param>
        /// <param name="number">A number that determines the singular or plural form to choose from.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public virtual IHtmlString Hn(string singular, string plural, long number, params object[] formatArgs)
            => MvcHtmlString.Create(Tn(singular, plural, number, formatArgs));

        /// <summary>
        /// Returns the singular or plural form of a contextual localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="context">The context to use.</param>
        /// <param name="singular">The identifier of the singular form of the translation.</param>
        /// <param name="plural">The identifier of the plural form of the translation.</param>
        /// <param name="count">A number that determines the singular or plural form to choose from.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public virtual IHtmlString Hxn(string context, string singular, string plural, long count, params object[] formatArgs)
            => MvcHtmlString.Create(Txn(context, singular, plural, count, formatArgs));

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
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
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
        public virtual string Txn(string context, string singular, string plural, long count, params object[] formatArgs)
            => TranslationManager.TranslatePlural(Culture, context, singular, plural, count, formatArgs);

        #endregion

        #endregion

        /// <summary>
        /// Initializes the current page.
        /// </summary>
        protected override void InitializePage()
        {
            Culture = Thread.CurrentThread.CurrentUICulture.Name;

            base.InitializePage();

            // hook up this event when translation is requested the first time and initialization occurs
            TranslationManager.TranslationsLoading += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(TranslationManager.PoFilesDirectory))
                {
                    TranslationManager.PoFilesDirectory = Server.MapPath("~/App_Data/languages");
                }
            };
        }

        /// <summary>
        /// Invalidates any previously loaded translation files.
        /// </summary>
        protected void ResetTranslations() => TranslationManager.ResetTranslations();
    }
}
