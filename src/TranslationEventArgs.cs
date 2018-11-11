using System;
using System.Collections.Generic;

namespace Polib.Net
{
    /// <summary>
    /// Represents the base for classes that contain data for translation events.
    /// </summary>
    public class TranslationEventArgs : EventArgs
    {
        #region constructors

        /// <summary>
        /// Initializes a new instace of the <see cref="TranslationEventArgs"/> class.
        /// </summary>
        public TranslationEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationEventArgs"/> class using the specified parameter.
        /// </summary>
        /// <param name="cancel">true to cancel the event; otherwise, false.</param>
        public TranslationEventArgs(bool cancel)
        {
            Cancel = cancel;
        }

        #endregion

        #region public properties
        
        /// <summary>
        /// Determines whether the event should be canceled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets a collection of <see cref="ICatalog"/> elements.
        /// </summary>
        public IList<ICatalog> Catalogs { get; set; }

        #endregion
    }
}
