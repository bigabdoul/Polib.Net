using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Polib.Net;
using Polib.NetCore.Web.Models;
using System.Diagnostics;

namespace Polib.NetCore.Web.Controllers
{
    public class AppController : Controller
    {
        readonly IConfiguration _configuration;

        public IConfigurationSection AppSettings { get; }

        TranslationSettings _translationSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppController"/> class using the specified parameter.
        /// </summary>
        /// <param name="configuration">The injected configuration</param>
        public AppController(IConfiguration configuration, IOptions<TranslationSettings> translationSettings)
        {
            _configuration = configuration;
            _translationSettings = translationSettings.Value;
            AppSettings = _configuration.GetSection("AppSettings");
        }

        /// <summary>
        /// Returns the Error view.
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Called before the action method is invoked, sets up the right theme.
        /// </summary>
        /// <param name="context">The action executing context.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var theme = Request.Query["theme"].ToString();

            if (!string.IsNullOrEmpty(theme))
            {
                ViewBag.Theme = theme;
                HttpContext.Session.SetString("Theme", theme);
            }
            else
            {
                ViewBag.Theme = HttpContext.Session.GetString("Theme");
            }

            base.OnActionExecuting(context);
        }

        #region protected

        /// <summary>
        /// Returns a shorter reference name to the <see cref="TranslationManager.Instance"/> property.
        /// </summary>
        protected TranslationManager Manager => TranslationManager.Instance;

        /// <summary>
        /// Translates a text in the singular form.
        /// </summary>
        /// <param name="singular">The text to translate.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        protected string T(string singular, params object[] formatArgs)
            => Manager.Translate(Domain, singular, formatArgs);

        /// <summary>
        /// Returns a contextual localized string from a translated.po file, optionally formatting the result.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="singular">The identifier of the localized string.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        protected string Tx(string context, string singular, params object[] formatArgs)
            => Manager.Translate(Domain, context, singular, formatArgs);

        protected string Tn(string singular, string plural, long count, params object[] formatArgs)
            => Manager.TranslatePlural(Domain, singular, plural, count, formatArgs);

        protected string Txn(string context, string singular, string plural, long count, params object[] formatArgs)
            => Manager.TranslatePlural(Domain, context, singular, plural, count, formatArgs);

        protected string Domain => System.Threading.Thread.CurrentThread.CurrentUICulture.Name;

        /// <summary>
        /// Determines whether the HTTP request is an AJAX request.
        /// </summary>
        /// <returns></returns>
        protected bool IsAjaxRequest => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        protected bool BackupOriginalCatalog => _translationSettings.BackupCatalogsBeforeSaving;

        protected bool WordWrapCommentReferences => _translationSettings.WordWrapCommentReferences;

        #endregion
    }
}
