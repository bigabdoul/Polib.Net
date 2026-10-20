using Microsoft.Extensions.Options;
using Polib.Net;
using Polib.Net.IO;
using Polib.Net.Synchronization;
using Polib.Net.Web.Models;
using Polib.NetCore.Mvc;
using Polib.NetCore.Mvc.Models;
using Polib.Net.Web;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add services to the container.
services.AddControllersWithViews();

Setup.TranslationSettings = new TranslationSettings();

builder.Configuration.GetSection(nameof(TranslationSettings)).Bind(Setup.TranslationSettings);

services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

services.AddDistributedMemoryCache();

services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
});

services
    .AddScoped<ICatalog, Catalog>()
    .AddScoped<ITranslation, Translation>()
    .AddScoped<IPoFileWriter, PoFileWriter>()
    .AddScoped<IUpdatedCatalog, UpdatedCatalog>()
    .AddScoped<IUpdatedTranslation, UpdatedTranslation>()
    .AddTransient<IHttpContextAccessor, HttpContextAccessor>()
    .AddTransient<IRazorPageTranslationManager, RazorTranslationManager>()
    .Configure<TranslationSettings>(builder.Configuration.GetSection(nameof(TranslationSettings)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

/*
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
*/
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseSession();

//app.UseMvc(routes =>
//{
//    routes.MapRoute(
//        name: "default",
//        template: "{controller=Home}/{action=Index}/{id?}");
//});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Setup.SetupTranslationManager(builder.Environment);

app.Run();

namespace Polib.Net.Web
{
    class Setup
    {
        public static TranslationSettings? TranslationSettings;

        #region helpers

        internal static void SetupTranslationManager(IWebHostEnvironment? env)
        {
            var mgr = TranslationManager.Instance;

            // include comments
            mgr.SkipComments = false;

            // event handler for setting the translation files directory
            mgr.TranslationsLoading += (s, e) => SetPoFilesDirectory(env);

            // initial setup
            SetPoFilesDirectory(env);

            // get the culture from settings or fallback to English
            var culture = TranslationSettings?.Culture ?? "en";

            // preload translations, don't wait for the first translation to occur
            mgr.LoadTranslationsAsync(culture).ConfigureAwait(false);
            mgr.FileWatcher.TimerPeriodInSeconds = true == env?.IsDevelopment() ? 10 : 600;
            mgr.FileWatcher.MonitorFileChanges = true;
        }

        private static void SetPoFilesDirectory(IWebHostEnvironment? env)
        {
            // set the directory path containing the .po translation files
            var path = true == env?.IsDevelopment()
                ? Path.Combine(env?.ContentRootPath ?? string.Empty, "wwwroot")
                : string.Empty;

            TranslationManager.Instance.PoFilesDirectory = Path.Combine(path, "languages");
        }

        #endregion

    }
}