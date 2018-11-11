using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polib.Net;
using Polib.Net.IO;
using Polib.Net.Synchronization;
using Polib.NetCore.Mvc;
using Polib.NetCore.Web.Models;
using System;
using System.IO;

namespace Polib.NetCore.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        public IConfiguration Configuration { get; }

        public IHostingEnvironment HostingEnvironment { get; }

        public TranslationSettings TranslationSettings { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
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

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services
                .AddScoped<ICatalog, Catalog>()
                .AddScoped<ITranslation, Translation>()
                .AddScoped<IPoFileWriter, PoFileWriter>()
                .AddScoped<IUpdatedCatalog, UpdatedCatalog>()
                .AddScoped<IUpdatedTranslation, UpdatedTranslation>()
                .AddTransient<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IRazorPageTranslationManager, RazorPageTranslationManager>()
                .Configure<TranslationSettings>(Configuration.GetSection(nameof(TranslationSettings)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IOptions<TranslationSettings> translationOptions)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            TranslationSettings = translationOptions.Value;
            SetupTranslationManager();
        }

        #region helpers

        private void SetupTranslationManager()
        {
            var mgr = TranslationManager.Instance;

            // include comments
            mgr.SkipComments = false;

            // event handler for setting the translation files directory
            mgr.TranslationsLoading += (s, e) => SetPoFilesDirectory();

            // initial setup
            SetPoFilesDirectory();

            // get the culture from settings or fallback to English
            var culture = TranslationSettings?.Culture ?? "en";

            // preload translations, don't wait for the first translation to occur
            mgr.LoadTranslationsAsync(culture).ConfigureAwait(false);
            mgr.FileWatcher.TimerPeriodInSeconds = HostingEnvironment.IsDevelopment() ? 10 : 600;
            mgr.FileWatcher.MonitorFileChanges = true;
        }

        private void SetPoFilesDirectory()
        {
            // set the directory path containing the .po translation files
            var path = HostingEnvironment.IsDevelopment() 
                ? Path.Combine(HostingEnvironment.ContentRootPath, "wwwroot") 
                : string.Empty;

            TranslationManager.Instance.PoFilesDirectory = Path.Combine(path, "languages");
        }

        #endregion
    }
}
