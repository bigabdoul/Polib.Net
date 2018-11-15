using Microsoft.AspNetCore.Http;
using System.Threading;

namespace Polib.NetCore.Mvc.Models
{
    /// <summary>
    /// Makes sure that the right culture is picked-up for each request.
    /// </summary>
    public sealed class RazorTranslationManager : RazorPageTranslationManager
    {
        readonly HttpContext _httpContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="RazorTranslationManager"/> class using the specified parameter.
        /// </summary>
        /// <param name="contextAccessor">An object used to access the current HTTP context.</param>
        public RazorTranslationManager(IHttpContextAccessor contextAccessor)
        {
            _httpContext = contextAccessor.HttpContext;
        }

        /// <summary>
        /// Gets the current UI culture info name of the current thread.
        /// </summary>
        public override string Culture
        {
            get
            {
                var thread = Thread.CurrentThread;
                var lang = _httpContext.Request.Query["lang"].ToString();

                if (!string.IsNullOrEmpty(lang))
                {
                    _httpContext.Session.SetString("CurrentCulture", lang);
                }
                else
                {
                    lang = _httpContext.Session.GetString("CurrentCulture");
                }

                if (!string.IsNullOrEmpty(lang) && 
                    !string.Equals(lang, thread.CurrentUICulture.Name, System.StringComparison.OrdinalIgnoreCase))
                {
                    thread.CurrentUICulture = new System.Globalization.CultureInfo(lang);
                }

                return thread.CurrentUICulture.Name;
            }
        }
    }
}
