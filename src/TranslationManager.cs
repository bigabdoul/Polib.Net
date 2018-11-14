using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Polib.Net
{
    using IO;
    using System.Globalization;
    using static Methods;

    /// <summary>
    /// Represents an object used for translation files management.
    /// </summary>
    public class TranslationManager : ITranslationManager, IDisposable
    {
        #region private fields

        private bool _disposed;
        private bool _catalogsLoaded = false;
        private ITranslationFileWatcher _fileWatcher;
        private readonly object objLock = new object();
        private readonly Dictionary<string, CultureInfo> _cultures = new Dictionary<string, CultureInfo>(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region protected fields

        /// <summary>
        /// Returns a reference to the cached translations.
        /// </summary>
        protected readonly HashSet<CachedTranslation> Cache = new HashSet<CachedTranslation>();

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationManager"/> class.
        /// </summary>
        public TranslationManager()
        {
        }

        #endregion
        
        #region static properties

        /// <summary>
        /// Returns the default instance of this <see cref="TranslationManager"/> class.
        /// </summary>
        public static TranslationManager Instance { get; } = new TranslationManager();

        #endregion
        
        #region public properties

        /// <summary>
        /// Gets or sets an object that watches translation file changes.
        /// </summary>
        public ITranslationFileWatcher FileWatcher
        {
            get
            {
                if (null == _fileWatcher)
                    _fileWatcher = new TranslationFileWatcher();
                return _fileWatcher;
            }
            set
            {
                _fileWatcher = value;
            }
        }

        /// <summary>
        /// Gets or sets a dictionary of <see cref="ICatalog"/> list grouped by culture containing all read catalogs.
        /// </summary>
        public IDictionary<string, IList<ICatalog>> Catalogs { get => FileWatcher.Catalogs; protected set => FileWatcher.Catalogs = value; }

        /// <summary>
        /// Gets or sets the .po files directory path.
        /// </summary>
        public string PoFilesDirectory
        {
            get => FileWatcher.Directory;
            set => FileWatcher.Directory = value;
        }

        /// <summary>
        /// Determines whether to skip comments when reading .po files. The default value is true.
        /// </summary>
        public virtual bool SkipComments { get => FileWatcher.SkipComments; set => FileWatcher.SkipComments = value; }

        /// <summary>
        /// Determines whether to include the regioncode2 for a specific culture when searching .po files. The default value is true.
        /// </summary>
        public bool IncludeCultureRegionCode2 { get; set; } = true;

        /// <summary>
        /// Determines whether to disable caching of translations.
        /// </summary>
        public bool CachingDisabled { get; set; }

        #endregion

        #region public events

        /// <summary>
        /// Event fired before translations are loaded.
        /// </summary>
        public event EventHandler<TranslationEventArgs> TranslationsLoading;

        #endregion

        #region public methods

        /// <summary>
        /// Returns a localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="culture">The culture or domain name of the translation to use.</param>
        /// <param name="singular">The identifier of the localized string.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public virtual string Translate(string culture, string singular, params object[] formatArgs)
            => Translate(culture, null, singular, formatArgs);

        /// <summary>
        /// Returns a contextual localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="culture">The culture or domain name of the translation to use.</param>
        /// <param name="context">The message context.</param>
        /// <param name="singular">The identifier of the localized string.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public virtual string Translate(string culture, string context, string singular, params object[] formatArgs)
        {
            var outcome = singular;
            var entry = new Translation { Singular = singular, Context = context };

            if (FindEntry(culture, entry, out var translation))
            {
                outcome = translation.Translations.Count > 0 ? translation.Translations[0] : singular;
            }

            if (formatArgs?.Length > 0)
            {
                outcome = string.Format(GetCultureInfo(culture), outcome, formatArgs);
            }

            return outcome;
        }

        /// <summary>
        /// Returns the singular or plural form of a localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="culture">The culture or domain name of the translation to use.</param>
        /// <param name="singular">The identifier of the singular form of the translation.</param>
        /// <param name="plural">The identifier of the plural form of the translation.</param>
        /// <param name="count">A number that determines the singular or plural form to choose from.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public virtual string TranslatePlural(string culture, string singular, string plural, long count, params object[] formatArgs)
            => TranslatePlural(culture, null, singular, plural, count, formatArgs);

        /// <summary>
        /// Returns the singular or plural form of a contextual localized string from a translated .po file, optionally formatting the result.
        /// </summary>
        /// <param name="culture">The culture or domain name of the translation to use.</param>
        /// <param name="context">The message context.</param>
        /// <param name="singular">The identifier of the singular form of the translation.</param>
        /// <param name="plural">The identifier of the plural form of the translation.</param>
        /// <param name="count">A number that determines the singular or plural form to choose from.</param>
        /// <param name="formatArgs">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public virtual string TranslatePlural(string culture, string context, string singular, string plural, long count, params object[] formatArgs)
        {
            var ci = GetCulture(culture);
            var outcome = string.Empty;
            var model = new Translation { Singular = singular, Plural = plural, Context = context };

            var index = 0;

            if (FindEntry(culture, model, out var entry))
            {
                outcome = entry.GetPlural(count, ci);
            }
            else
            {
                index = Plurals.PluralFormsEvaluator.GetPluralIndex(ci, count);
                outcome = index == 0 ? singular : plural;
            }

            if (formatArgs?.Length > 0)
            {
                outcome = string.Format(ci, outcome, formatArgs);
            }

            return outcome;
        }

        /// <summary>
        /// Invalidates any previously loaded translation files.
        /// </summary>
        public virtual void ResetTranslations()
        {
            lock (objLock)
            {
                Catalogs?.Clear();
                ClearCache();
                _cultures.Clear();
                _catalogsLoaded = false;
            }
        }

        /// <summary>
        /// Clears the translation cache.
        /// </summary>
        public virtual void ClearCache() => Cache.Clear();

        /// <summary>
        /// Ensures that translations for the specified culture are loaded asynchronously.
        /// </summary>
        /// <param name="culture">The culture or domain name of the translation to use.</param>
        /// <returns></returns>
        public virtual Task LoadTranslationsAsync(string culture) => Task.Run(() => EnsureTranslationsLoaded(culture));

        /// <summary>
        /// Releases all resources used by the current <see cref="TranslationManager"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region protected helpers

        /// <summary>
        /// Makes sure that all translation files have been loaded from the <see cref="PoFilesDirectory"/> path.
        /// </summary>
        /// <param name="culture">The culture or domain name of the translation to use.</param>
        protected virtual void EnsureTranslationsLoaded(string culture)
        {
            if (!_catalogsLoaded)
            {
                lock (objLock)
                {
                    if (!_catalogsLoaded)
                    {
                        var cancel = false;

                        if (TranslationsLoading != null)
                        {
                            var args = new TranslationEventArgs(cancel);
                            OnTranslationsLoading(args);

                            // has the event been canceled?
                            cancel = args.Cancel;

                            if (cancel && args.Catalogs != null)
                            {
                                Catalogs = args.Catalogs.GroupByCulture();
                                _catalogsLoaded = true;
                            }
                        }

                        if (!cancel)
                        {
                            if (!string.IsNullOrEmpty(PoFilesDirectory))
                            {
                                Catalogs = PoFileReader.ReadAll(PoFilesDirectory
                                    , culture
                                    , SkipComments
                                    , FileWatcher.IncludeSubdirectories
                                    , IncludeCultureRegionCode2).GroupByCulture();
                            }
                            else if (!(Catalogs?.Count > 0))
                            {
                                throw new InvalidOperationException("Translation catalogs have not been initialized.");
                            }

                            FileWatcher.CurrentCulture = culture;
                            _catalogsLoaded = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fires the <see cref="TranslationsLoading"/> event.
        /// </summary>
        protected virtual void OnTranslationsLoading(TranslationEventArgs args) => TranslationsLoading?.Invoke(this, args);

        /// <summary>
        /// Ensures that translation files are loaded and attempts to find the identified translation.
        /// </summary>
        /// <param name="culture">The culture or domain name of the translation to use.</param>
        /// <param name="model">The model whose <see cref="Translation.Key"/> property is used to retrieve a translation.</param>
        /// <param name="result">Returns the retrieved translation entry, if any.</param>
        /// <returns></returns>
        protected virtual bool FindEntry(string culture, ITranslation model, out ITranslation result)
        {
            var key = model.Key;

            if (string.IsNullOrEmpty(FileWatcher.CurrentCulture))
            {
                FileWatcher.CurrentCulture = culture;
            }
            else if (string.IsNullOrEmpty(culture))
            {
                culture = FileWatcher.CurrentCulture;
            }

            if (!_catalogsLoaded) EnsureTranslationsLoaded(culture);

            var useCache = !CachingDisabled;

            if (useCache && Cache.Find(culture, key, out result))
            {
                return true;
            }

            if (Catalogs.TryGetValue(culture, out var cats) && cats.Find(key, out result))
            {
                if (useCache) 
                    Cache.Add(new CachedTranslation(culture, key, result));
                return true;
            }

            // try the regioncode2
            var cinfo = GetCulture(culture);

            if (!cinfo.IsRegionCode2() && 
                Catalogs.TryGetValue(cinfo.TwoLetterISOLanguageName, out cats) && 
                cats.Find(key, out result))
            {
                if (useCache)
                    Cache.Add(new CachedTranslation(cinfo.TwoLetterISOLanguageName, key, result));
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Releases all resources used by this <see cref="TranslationManager"/>.
        /// </summary>
        /// <param name="disposing">true to release managed resources; otherwise, false.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                ResetTranslations();
                FileWatcher.Dispose();
            }

            _disposed = true;
        }

        #endregion

        #region private helpers

        private CultureInfo GetCulture(string culture)
        {
            if (_cultures.ContainsKey(culture))
            {
                return _cultures[culture];
            }
            var info = GetCultureInfo(culture);
            _cultures.Add(culture, info);
            return info;
        }

        #endregion
    }
}
