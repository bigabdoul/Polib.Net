using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polib.NetCore.Web.Extensions
{
    /// <summary>
    /// Provides extension methods for instances of the <see cref="String"/> class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns a substring with a specified maximum length.
        /// </summary>
        /// <param name="s">The string to truncate.</param>
        /// <param name="maxlen">The maximum length of the string to return.</param>
        /// <returns></returns>
        public static string Truncate(this string s, int maxlen)
        {
            if (s == null || s.Length < maxlen) return s;
            return s.Substring(0, maxlen) + "...";
        }

        /// <summary>
        /// Returns <paramref name="s"/> as an instance of the <see cref="HtmlString"/> class.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns></returns>
        public static IHtmlContent ToHtml(this string s) => null == s ? HtmlString.Empty : new HtmlString(s);

        /// <summary>
        /// Extract the value attribute from an HTML element contained in the given Mvc.MvcHtmlString.
        /// </summary>
        /// <param name="s">The <see cref="HtmlString"/> from which to extract the value.</param>
        /// <returns></returns>
        public static string ExtractInputValue(this IHtmlContent s)
        {
            string token = null;
            var m = System.Text.RegularExpressions.Regex.Match(s.ToString(), @"value\s*=\s*""(?<token>[\w\+-]+)""");
            if (m.Success)
            {
                token = m.Groups["token"].Value;
            }
            return token;
        }
    }
}
