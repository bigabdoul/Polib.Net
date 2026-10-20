using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Polib.Net.Collections
{
    /// <summary>
    /// Represents a dictionary of <see cref="ICatalog"/> collections grouped by culture.
    /// </summary>
    public class CatalogDictionary : IDictionary<string, IList<ICatalog>>
    {
        #region fields

        readonly ConcurrentDictionary<string, IList<ICatalog>> _dic =
            new ConcurrentDictionary<string, IList<ICatalog>>(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogDictionary"/> class.
        /// </summary>
        public CatalogDictionary()
        {
        }

        #endregion
        
        #region properties

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns></returns>
        public IList<ICatalog> this[string key]
        {
            get => _dic[key];
            set => _dic.TryAdd(key, value);
        }

        /// <summary>
        /// Gets a collection containing the keys in the catalog.
        /// </summary>
        public ICollection<string> Keys => _dic.Keys;

        /// <summary>
        ///  Gets a collection containing the values in the catalog.
        /// </summary>
        public ICollection<IList<ICatalog>> Values => _dic.Values;

        /// <summary>
        /// Gets the number of key/value pairs contained in the catalog.
        /// </summary>
        public int Count => _dic.Count;

        /// <summary>
        /// Returns false.
        /// </summary>
        public bool IsReadOnly => false;

        #endregion

        #region methods

        /// <summary>
        /// Attempts to add the specified key and value to the catalog.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        public void Add(string key, IList<ICatalog> value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            _dic.TryAdd(key, value);
        }

        /// <summary>
        /// Removes all keys and values from the current catalog.
        /// </summary>
        public void Clear() => _dic.Clear();

        /// <summary>
        /// Determines whether the catalog contains the specified key.
        /// </summary>
        /// <param name="culture">The culture to locate in the catalog.</param>
        /// <returns></returns>
        public bool ContainsKey(string culture) => _dic.ContainsKey(culture);

        /// <summary>
        /// Returns an enumerator that iterates through the dictionary.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, IList<ICatalog>>> GetEnumerator()
            => _dic.GetEnumerator();

        /// <summary>
        /// Removes the element with the specified key from the catalog.
        /// </summary>
        /// <param name="culture">The culture of the catalogs to remove.</param>
        /// <returns></returns>
        public bool Remove(string culture) => _dic.TryRemove(culture, out var value);

        /// <summary>
        /// Attempts to get the value associated with the specified key from the catalog.
        /// </summary>
        /// <param name="culture">The culture whose catalogs to get.</param>
        /// <param name="value">
        /// The value associated with the specified culture, if the culture is found; 
        /// otherwise, the default value for the type of the value parameter.
        /// </param>
        /// <returns></returns>
        public bool TryGetValue(string culture, out IList<ICatalog> value) => _dic.TryGetValue(culture, out value);

        #endregion
        
        #region implicit interface implementations

        /// <summary>
        /// Attempts to add the specified item to the catalog.
        /// </summary>
        /// <param name="item">The item to add.</param>
        void ICollection<KeyValuePair<string, IList<ICatalog>>>.
            Add(KeyValuePair<string, IList<ICatalog>> item) => Add(item.Key, item.Value);

        /// <summary>
        /// Removes the item with the specified key from the catalog.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns></returns>
        bool ICollection<KeyValuePair<string, IList<ICatalog>>>.
            Remove(KeyValuePair<string, IList<ICatalog>> item) => Remove(item.Key);

        /// <summary>
        /// Copies all the elements of the current one-dimensional array to the specified
        /// one-dimensional array starting at the specified destination array index. 
        /// The index is specified as a 32-bit integer.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the current array.</param>
        /// <param name="arrayIndex"> A 32-bit integer that represents the index in array at which copying begins.</param>
        void ICollection<KeyValuePair<string, IList<ICatalog>>>.
            CopyTo(KeyValuePair<string, IList<ICatalog>>[] array, int arrayIndex) 
            => _dic.ToArray().CopyTo(array, arrayIndex);

        /// <summary>
        /// Determines whether the catalog contains a key that matches the <paramref name="item"/>'s 
        /// <see cref="KeyValuePair{TKey, TValue}.Key"/> property.
        /// </summary>
        /// <param name="item">The item's key property to check.</param>
        /// <returns></returns>
        bool ICollection<KeyValuePair<string, IList<ICatalog>>>.
            Contains(KeyValuePair<string, IList<ICatalog>> item) => _dic.ContainsKey(item.Key);

        /// <summary>
        /// Returns an enumerator that iterates through the dictionary.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); 

        #endregion
    }
}
