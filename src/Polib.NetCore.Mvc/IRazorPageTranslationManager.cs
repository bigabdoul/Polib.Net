using Microsoft.AspNetCore.Html;

namespace Polib.NetCore.Mvc
{
    /// <summary>
    /// Specifies the contract required for classes that inject .po files translation capabilities into razor views.
    /// </summary>
    public interface IRazorPageTranslationManager
    {
        #region versions returning IHtmlContent

        /// <summary>
        /// Returns a localized HTML-encoded string from a 
        /// translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="singular">The identifier of the localized string.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        IHtmlContent H(string singular, params object[] formatArgs);

        /// <summary>
        /// Returns a contextual localized HTML-encoded string from a 
        /// translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="context">The context to use.</param>
        /// <param name="singular">The identifier of the singular form of the translation.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        IHtmlContent Hx(string context, string singular, params object[] formatArgs);

        /// <summary>
        /// Returns the singular or plural form of a localized HTML-encoded 
        /// string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="singular">The identifier of the singular form of the translation.</param>
        /// <param name="plural">The identifier of the plural form of the translation.</param>
        /// <param name="count">A number that determines the singular or plural form to choose from.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        IHtmlContent Hn(string singular, string plural, long count, params object[] formatArgs);

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
        IHtmlContent Hnx(string context, string singular, string plural, long count, params object[] formatArgs);

        #endregion

        #region equivalent versions returning string
        
        /// <summary>
        /// Returns a localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="singular">The identifier of the localized string.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        string T(string singular, params object[] formatArgs);
        
        /// <summary>
        /// Returns a contextual localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="context">The context to use.</param>
        /// <param name="singular">The identifier of the singular form of the translation.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        string Tx(string context, string singular, params object[] formatArgs);
        
        /// <summary>
        /// Returns the singular or plural form of a localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="singular">The identifier of the singular form of the translation.</param>
        /// <param name="plural">The identifier of the plural form of the translation.</param>
        /// <param name="count">A number that determines the singular or plural form to choose from.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        string Tn(string singular, string plural, long count, params object[] formatArgs);
        
        /// <summary>
        /// Returns the singular or plural form of a contextual localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="context">The context to use.</param>
        /// <param name="singular">The identifier of the singular form of the translation.</param>
        /// <param name="plural">The identifier of the plural form of the translation.</param>
        /// <param name="count">A number that determines the singular or plural form to choose from.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        string Tnx(string context, string singular, string plural, long count, params object[] formatArgs);

        #endregion
    }
}