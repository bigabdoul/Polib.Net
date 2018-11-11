namespace Polib.NetCore.Web.Models
{
    /// <summary>
    /// Encapsulates translation-specific application configuration settings.
    /// </summary>
    public class TranslationSettings
    {
        /// <summary>
        /// Gets or sets the culture to use for the application.
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// Determines whether to back up catalogs before they're being updated.
        /// </summary>
        public bool BackupCatalogsBeforeSaving { get; set; }

        /// <summary>
        /// Determines whether to word-wrap comment references in translation files.
        /// </summary>
        public bool WordWrapCommentReferences { get; set; }
    }
}
