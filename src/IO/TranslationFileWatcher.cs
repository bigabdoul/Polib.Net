using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Polib.Net.IO
{
    /// <summary>
    /// Represents an object that watches the file system for changes occuring in translation files.
    /// </summary>
    public class TranslationFileWatcher : ITranslationFileWatcher, IDisposable
    {
        #region fields

        private bool _insync;
        private bool _disposed;
        private readonly object objLock = new object();

        /// <summary>
        /// Returns a reference to the internal timer that watches for file changes.
        /// </summary>
        protected readonly Timer WatcherTimer;

        /// <summary>
        /// Returns a reference to the <see cref="FileSystemWatcher"/> that tracks changes made 
        /// to translation files located under the <see cref="Directory"/> directory.
        /// </summary>
        public readonly FileSystemWatcher FileWatcher;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationFileWatcher"/> class using the specified parameters.
        /// </summary>
        public TranslationFileWatcher()
        {
            FileWatcher = new FileSystemWatcher
            {
                Filter = "*.po",
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                IncludeSubdirectories = true,
            };

            FileWatcher.Changed += OnFileSystemChange;
            FileWatcher.Created += OnFileSystemChange;
            FileWatcher.Deleted += OnFileSystemChange;
            FileWatcher.Error += (s, e) => { }; // ignore errors

            // create new timer in disabled state
            WatcherTimer = new Timer(PollFileChanges, null, Timeout.Infinite, Timeout.Infinite);
        }

        #endregion

        #region events

        /// <summary>
        /// Event fired after a <see cref="FileSystemWatcher"/> event occured and translations files were synchronized.
        /// </summary>
        public event EventHandler<TranslationEventArgs> TranslationFilesSynchronized;

        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the synchronization root for this <see cref="TranslationFileWatcher"/> instance.
        /// </summary>
        public virtual object SyncRoot { get; set; } = new object();

        /// <summary>
        /// Gets or sets the dictionary of catalogs grouped by culture.
        /// </summary>
        public virtual IDictionary<string, IList<ICatalog>> Catalogs { get; set; }

        /// <summary>
        /// Determines whether to watch for translation file changes in <see cref="Directory"/>. The default value is false.
        /// <para>
        /// If directory change monitoring is enabled, translations are synchronized immediately as soon as the 
        /// <see cref="FileSystemWatcher.Changed"/>, and <see cref="FileSystemWatcher.Created"/> events are fired.
        /// </para>
        /// </summary>
        public virtual bool MonitorFileChanges
        {
            get => FileWatcher.EnableRaisingEvents;
            set
            {
                if (value != FileWatcher.EnableRaisingEvents)
                {
                    FileWatcher.EnableRaisingEvents = value;
                    if (value)
                    {
                        // enable the timer
                        WatcherTimer.Change(0, TimerPeriodInSeconds * 1000);
                    }
                    else
                    {
                        // disable
                        WatcherTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether to include subdirectories when searching for .po files. The default value is true.
        /// </summary>
        public virtual bool IncludeSubdirectories
        {
            get => FileWatcher.IncludeSubdirectories;
            set => FileWatcher.IncludeSubdirectories = value;
        }

        /// <summary>
        /// Gets or sets the time interval (seconds) between invocations of the callback method that watches for translation file changes.
        /// To disable the timer, set this value to <see cref="Timeout.Infinite"/>. The default value is 600 seconds (10 minutes).
        /// </summary>
        public virtual int TimerPeriodInSeconds { get; set; } = 600;

        /// <summary>
        /// Gets or sets the directory path to watch.
        /// </summary>
        public virtual string Directory { get => FileWatcher.Path; set => FileWatcher.Path = value; }

        /// <summary>
        /// Determines whether to skip comments when reading changed translation files.
        /// </summary>
        public virtual bool SkipComments { get; set; }

        /// <summary>
        /// Gets or sets the current culture.
        /// </summary>
        public virtual string CurrentCulture { get; set; }

        #endregion

        /// <summary>
        /// Releases all resources used by the current <see cref="TranslationFileWatcher"/>.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Represents a collective handler for supported events of the internal instance of the <see cref="FileSystemWatcher"/> class.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        protected virtual void OnFileSystemChange(object sender, FileSystemEventArgs e)
        {
            try
            {
                // if changes happened in the directory, sync the translations dictionary
                if (null != Catalogs && !string.IsNullOrWhiteSpace(CurrentCulture))
                {
                    lock (SyncRoot ?? objLock)
                    {
                        var success = false;

                        // assume the current culture
                        var culture = CurrentCulture;

                        if (Catalogs.FindCatalog(e.FullPath, out var catalog))
                        {
                            // knowing that this event may fire several times for the same file, avoid reparsing;
                            if (DateTime.Now.Subtract(catalog.LastAccessTime).TotalSeconds < 5d)
                                // ignore changes that occured within the last couple seconds
                                return;

                            // set the right culture
                            culture = catalog.Culture.Name;

                            // reading a ~10.000 (ten thousand) lines translation file takes about 100 ms!
                            // (on my dev machine) and shouldn't impact performance; in fact it's preferable
                            // to read the file again than trying to merge the changes back into the dictionary
                            Catalogs.RemoveCatalog(catalog);
                            success = true;
                        }

                        if (e.ChangeType != WatcherChangeTypes.Deleted)
                        {
                            success = false;

                            // read translations from the file
                            catalog = PoFileReader.Read(e.FullPath, culture, SkipComments);

                            // does the culture exist in the dictionary?
                            if (Catalogs.TryGetValue(culture, out var domainCatalogs))
                            {
                                // update the collection of readers for this culture
                                domainCatalogs.Add(catalog);
                                success = true;
                            }
                            else
                            {
                                Catalogs.Add(culture, new List<ICatalog> { catalog });
                                success = true;
                            }
                        }

                        // notify changes?
                        if (success) OnTranslationFilesSynchronized(catalog);
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Fires the <see cref="TranslationFilesSynchronized"/> event.
        /// </summary>
        /// <param name="catalogs">The catalogs that were created or refreshed as a result of synchronization.</param>
        protected virtual void OnTranslationFilesSynchronized(params ICatalog[] catalogs)
            => TranslationFilesSynchronized?.Invoke(this, new TranslationEventArgs { Catalogs = catalogs, });

        /// <summary>
        /// Releases all resources used by this <see cref="TranslationFileWatcher"/>.
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
                if (FileWatcher != null)
                {
                    FileWatcher.Changed -= OnFileSystemChange;
                    FileWatcher.Created -= OnFileSystemChange;
                    FileWatcher.Deleted -= OnFileSystemChange;
                    FileWatcher.Dispose();
                }
            }

            _disposed = true;
        }

        #region private helpers

        private void PollFileChanges(object state)
        {
            if (_insync || !MonitorFileChanges) return;
            if (Catalogs == null || string.IsNullOrWhiteSpace(CurrentCulture)) return;

            try
            {
                lock (SyncRoot ?? objLock)
                {
                    if (!_insync)
                    {
                        _insync = true;
                        var syncList = new HashSet<Catalog>();

                        foreach (var collection in Catalogs.Values)
                        {
                            for (int i = 0; i < collection.Count; i++)
                            {
                                var rdr = collection[i];
                                var fn = rdr.FileName;

                                if (string.IsNullOrEmpty(fn)) continue;

                                try
                                {
                                    if (new FileInfo(fn).LastWriteTime > rdr.LastAccessTime)
                                    {
                                        var reader = PoFileReader.Read(fn, CurrentCulture, SkipComments);
                                        collection[i] = reader;
                                        syncList.Add(reader);
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }

                        if (TranslationFilesSynchronized != null)
                        {
                            OnTranslationFilesSynchronized(syncList.ToArray());
                        }

                        _insync = false;
                    }
                }
            }
            catch
            {
                _insync = false;
            }
        }

        #endregion
    }
}
