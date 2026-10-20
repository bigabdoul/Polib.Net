namespace Polib.Net
{
    /// <summary>
    /// Represents a wrapper around an instance of a class that implements the <see cref="ITranslation"/> class.
    /// This is useful when caching previously translated items.
    /// </summary>
    public struct CachedTranslation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedTranslation"/> class using the specified parameters.
        /// </summary>
        /// <param name="culture">The culture of the <paramref name="translation"/>.</param>
        /// <param name="key">The unique key of the translation.</param>
        /// <param name="translation">The translation referenced by this <see cref="CachedTranslation"/>.</param>
        public CachedTranslation(string culture, string key, ITranslation translation)
        {
            Key = culture + key;
            Translation = translation;
        }

        /// <summary>
        /// Gets the key of this wrapper.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the translation of this wrapper.
        /// </summary>
        public ITranslation Translation { get; }
    }
}
