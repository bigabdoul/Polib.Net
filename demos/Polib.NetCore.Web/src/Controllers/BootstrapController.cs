using Bootstrap.Themes;
#if ASPNETCORE
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Mvc;
#endif
using System;

namespace Polib.NetCore.Web.Controllers
{
    public class BootstrapController : Controller
    {
        /// <summary>
        /// Returns a Bootstrap CSS theme file.
        /// </summary>
        /// <param name="id">The identifier of the default built-in theme to return.</param>
        /// <param name="theme">The name of another built-in theme to return. This has a higher precedence over <paramref name="id"/>.</param>
        /// <returns></returns>
        public IActionResult Themes(BuiltInThemeName? id,
#if ASPNETCORE
            [FromQuery(Name = "theme")]
#endif
        string theme)
        {
            if (!string.IsNullOrEmpty(theme) && Enum.TryParse<BuiltInThemeName>(theme, true, out var result))
            {
                id = result;
            }

            if (id.HasValue && Theme.FromResource(id.Value, out var t))
            {
                return File(System.Text.Encoding.UTF8.GetBytes(t.Content), "text/css");
            }

            return NotFound();
        }

    }
}
