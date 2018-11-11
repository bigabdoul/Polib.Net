using System.Collections.Generic;

namespace Polib.Net.Synchronization
{
    /// <summary>
    /// Specifies the data contract required for classes 
    /// that represent an updated catalog of translated items.
    /// </summary>
    public interface IUpdatedCatalog : ICatalog
    {
        /// <summary>
        /// Gets or sets the unique of the catalog.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the collection of updated translations.
        /// </summary>
        IList<UpdatedTranslation> Items { get; set; }
    }
}
