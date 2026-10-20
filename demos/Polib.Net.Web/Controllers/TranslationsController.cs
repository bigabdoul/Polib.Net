using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Polib.Net;
using Polib.Net.IO;
using Polib.Net.Synchronization;
using Polib.Net.Web.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Polib.Net.Web.Controllers
{
    public class TranslationsController : AppController
    {
        public TranslationsController(
            Microsoft.Extensions.Configuration.IConfiguration configuration, 
            Microsoft.Extensions.Options.IOptions<TranslationSettings> translationSettings) 
            : base(configuration, translationSettings) {}

        public IActionResult Index() => View(Manager.Catalogs);

        public IActionResult Catalog(string id, int? newentries)
        {
            if (!FindCatalog(id, out var catalog))
            {
                ViewBag.Message = T("Catalog id '{0}' not found!", id);
                //return NotFound();
            }

            ViewBag.NewEntries = newentries;
            return View(catalog);
        }

        public IActionResult Download(string id)
        {
            if (FindCatalog(id, out var catalog))
            {
                var fname = Path.GetFileName(catalog?.FileName);
                string? contentType = null;
                catalog?.GetHeader("Content-Type", out contentType);
                return File($"~/languages/{fname}", contentType ?? "text/plain", fname);
            }
            return NotFound();
        }

        /// <summary>
        /// Ajax request to get a catalog identified by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The identifier of the catalog.</param>
        /// <returns></returns>
        public IActionResult GetCatalog(string id)
        {
            FindCatalog(id, out var catalog);
            return Json(ToUpdatable(catalog));
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult UpdateCatalog([FromBody] UpdatedCatalog model)
        {
            if (FindCatalog(model.Id, out var catalog))
            {
                var values = catalog?.Entries.Values;
                var array = new ITranslation[values?.Count ?? 0];
                values?.CopyTo(array, 0);

                if (model.Items.MergeChanges(array))
                {
                    bool success = catalog.SaveChanges(BackupOriginalCatalog, WordWrapCommentReferences);
                    var message = success ? string.Empty : T("Could not save catalog changes back to file.");
                    return Json(new { success, message });
                }
                else
                {
                    return Json(new { success = false, message = T("No change has been detected.") });
                }
            }
            return Json(new { success = false, message = T("Could not find the catalog with specified catalog.") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MergeCatalog(string id)
        {
            try
            {
                if (FindCatalog(id, out var current))
                {
                    var count = 0;
                    var files = Request.Form.Files.Where(f => f.Length > 0L).ToArray();

                    if (files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            using var fstream = file.OpenReadStream();
                            using var tr = new StreamReader(fstream, System.Text.Encoding.UTF8);
                            var catalog = PoFileReader.Read(tr, Domain, enforceCultureInfo: false);
                            count += current?.MergeWith(catalog) ?? 0;
                        }
                    }

                    // show the catalog to update
                    return RedirectToAction(nameof(Catalog), new { id, newentries = count });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return Error();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refresh()
        {
            Manager.ResetTranslations();
            await Manager.LoadTranslationsAsync(Domain);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Renders and returns the Create view.
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UpdatedCatalog model)
        {
            if (ModelState.IsValid)
            {

            }

            return View(model);
        }


        #region helpers

        bool FindCatalog(string fileId, out ICatalog? catalog)
        {
            catalog = null;
            return true == Manager?.Catalogs?.FindById(fileId, out catalog);
        }

        static ICatalog ToUpdatable(ICatalog? catalog)
        {
            if (null == catalog) catalog = new Catalog();
            var row = 1;
            var cat = new UpdatedCatalog
            {
                Id = catalog.FileId,
                PluralCount = catalog.PluralCount,
                Items = catalog.Entries.Values.Select(t => new UpdatedTranslation(row++)
                {
                    UniqueKey = t.Key,
                    Context = t.Context,
                    Singular = t.Singular,
                    Plural = t.Plural,
                    Translations = t.Translations,
                    ExtractedComments = t.ExtractedComments,
                    Flags = t.Flags,
                    References = t.References,
                    TranslatorComments = t.TranslatorComments,
                }).ToArray(),
            };

            var headers = cat.CreateHeaders();

            foreach (var hdr in catalog.Headers)
            {
                headers.Add(hdr.Key, hdr.Value);
            }

            return cat;
        }
        
        #endregion
    }
}
