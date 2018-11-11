using System;
using System.Collections.Generic;
using System.Linq;

namespace Polib.Net
{
    using Collections;

    /// <summary>
    /// Provides extension methods for translation file items.
    /// </summary>
    public static class TranslationExtensions
    {
        /// <summary>
        /// Transforms the collection into a dictionary containing collections of <see cref="ICatalog"/> elements grouped by culture.
        /// </summary>
        /// <param name="collection">The collection of <see cref="ICatalog"/> to transform.</param>
        /// <returns></returns>
        public static CatalogDictionary GroupByCulture(this IEnumerable<ICatalog> collection)
        {
            // dictionary to store all groups for the culture name
            var catalogs = new CatalogDictionary();

            // group all by catalogs by the variation of the specified culture name
            foreach (var g in collection.GroupBy(r => r.Culture.Name))
            {
                // add catalogs to a list for each language group
                var list = new List<ICatalog>();

                foreach (var cat in g)
                {
                    list.Add(cat);
                }

                catalogs.Add(g.Key, list);
            }

            return catalogs;
        }

        /// <summary>
        /// Searches the dictionary for a <see cref="Catalog"/> read from the specified file name.
        /// </summary>
        /// <param name="dictionary">The dictionary to search.</param>
        /// <param name="fileName">The name of the file used to read the catalog.</param>
        /// <param name="catalog">Returns the <see cref="Catalog"/>.</param>
        /// <returns></returns>
        public static bool FindCatalog(this IDictionary<string, IList<ICatalog>> dictionary, string fileName, out ICatalog catalog)
        {
            // the dictionary contains a collection of catalogs
            foreach (var catalogs in dictionary.Values)
            {
                foreach (var cat in catalogs)
                {
                    if (string.Equals(cat.FileName, fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        catalog = cat;
                        return true;
                    }
                }
            }

            catalog = null;
            return false;
        }

        /// <summary>
        /// Attempts to remove the specified catalog from the dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary from which to remove <paramref name="catalog"/>.</param>
        /// <param name="catalog">The catalog to remove from <paramref name="dictionary"/>.</param>
        /// <returns></returns>
        public static bool RemoveCatalog(this IDictionary<string, IList<ICatalog>> dictionary, ICatalog catalog)
        {
            var fn = catalog.FileName;
            var hasFile = !string.IsNullOrEmpty(fn);

            foreach (var collection in dictionary.Values)
            {
                for (int i = 0; i < collection.Count; i++)
                {
                    var cat = collection[i];

                    if (hasFile)
                    {
                        if (string.Equals(cat.FileName, fn, StringComparison.OrdinalIgnoreCase))
                        {
                            collection.RemoveAt(i);
                            return true;
                        }
                    }
                    else if (Equals(cat, catalog))
                    {
                        collection.Remove(cat);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a collection of all catalogs, optionally of the specified culture, contained in the dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary whose catalogs are to be returned.</param>
        /// <param name="culture">The culture for which to return catalogs. Can be null.</param>
        /// <returns></returns>
        public static IEnumerable<ICatalog> GetCatalogs(this IDictionary<string, IList<ICatalog>> dictionary, 
            string culture = null)
        {
            var withCulture = !string.IsNullOrEmpty(culture);
            
            foreach (var collection in dictionary.Values)
            {
                foreach (var po in collection)
                {
                    if (withCulture)
                    {
                        if (string.Equals(po.Culture.Name, culture, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return po;
                        }
                    }
                    else
                    {
                        yield return po;
                    }
                }
            }
        }

        /// <summary>
        /// Finds the first catalog with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="dictionary">The dictionary to search.</param>
        /// <param name="id">The unique identifier of the catalog to find.</param>
        /// <param name="result">Returns the matched catalog, or null.</param>
        /// <returns></returns>
        public static bool FindById(this IDictionary<string, IList<ICatalog>> dictionary, string id, out ICatalog result)
        {
            foreach (var collection in dictionary.Values)
            {
                if (collection.FindById(id, out result))
                {
                    return true;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Finds the first catalog with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="collection">The collection to search.</param>
        /// <param name="id">The unique identifier of the catalog to find.</param>
        /// <param name="result">Returns the matched catalog, or null.</param>
        /// <returns></returns>
        public static bool FindById(this IEnumerable<ICatalog> collection, string id, out ICatalog result)
        {
            result = collection.Where(c => c.FileId == id).FirstOrDefault();
            return result != null;
        }

        /// <summary>
        /// Finds the first translation that matches the specified key in the catalog collection.
        /// </summary>
        /// <param name="collection">The collection to search.</param>
        /// <param name="key">The unique key of the cached translation to find.</param>
        /// <param name="result">Returns the matched translation, or null.</param>
        /// <returns></returns>
        public static bool Find(this IEnumerable<ICatalog> collection, string key, out ITranslation result)
        {
            result = null;

            foreach (var catalog in collection)
            {
                if (catalog.Entries.ContainsKey(key))
                {
                    result = catalog.Entries[key];
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds the first cached translation that matches the specified key in the collection.
        /// </summary>
        /// <param name="collection">The collection to search.</param>
        /// <param name="culture">The culture of the translation to find.</param>
        /// <param name="key">The unique key of the cached translation to find.</param>
        /// <param name="result">Returns the matched translation, or null.</param>
        /// <returns></returns>
        public static bool Find(this IEnumerable<CachedTranslation> collection, string culture, string key, out ITranslation result)
        {
            key = culture + key;

            foreach (var item in collection)
            {
                if (item.Key == key)
                {
                    result = item.Translation;
                    return true;
                }
            }
            result = null;
            return false;
        }
    } // end class
}
