using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Polib.Net.IO
{
    /// <summary>
    /// Specifies the contract required for classes that watch the file system for changes occuring in translation files.
    /// </summary>
    public interface ITranslationFileWatcher : IDisposable
    {
        /// <summary>
        /// Gets or sets the dictionary of catalogs grouped by culture.
        /// </summary>
        IDictionary<string, IList<ICatalog>> Catalogs { get; set; }

        /// <summary>
        /// Gets or sets the current culture.
        /// </summary>
        string CurrentCulture { get; set; }

        /// <summary>
        /// Gets or sets the directory path to watch.
        /// </summary>
        string Directory { get; set; }

        /// <summary>
        /// Determines whether to include subdirectories when searching for .po files. The default value is true.
        /// </summary>
        bool IncludeSubdirectories { get; set; }

        /// <summary>
        /// Determines whether to watch for translation file changes in <see cref="Directory"/>. The default value is false.
        /// <para>
        /// If directory change monitoring is enabled, translations are synchronized immediately as soon as the 
        /// <see cref="FileSystemWatcher.Changed"/>, and <see cref="FileSystemWatcher.Created"/> events are fired.
        /// </para>
        /// </summary>
        bool MonitorFileChanges { get; set; }

        /// <summary>
        /// Determines whether to skip comments when reading changed translation files.
        /// </summary>
        bool SkipComments { get; set; }

        /// <summary>
        /// Gets or sets the time interval (seconds) between invocations of the callback method that watches for translation file changes.
        /// To disable the timer, set this value to <see cref="Timeout.Infinite"/>. The default value is 600 seconds (10 minutes).
        /// </summary>
        int TimerPeriodInSeconds { get; set; }

        /// <summary>
        /// Event fired after a <see cref="FileSystemWatcher"/> event occured and translations files were synchronized.
        /// </summary>
        event EventHandler<TranslationEventArgs> TranslationFilesSynchronized;
    }
}