using System.Collections.Generic;

namespace Polib.Net.Plurals
{
    /// <summary>
    /// Specifies the contract required by a class that provides gettext Plural-Forms parser functionality.
    /// </summary>
    public interface IPluralFormsParser
    {
        /// <summary>
        /// Parse a Plural-Forms string into tokens.
        /// </summary>
        /// <returns></returns>
        IList<KeyValuePair<string, string>> Parse();
    }
}
