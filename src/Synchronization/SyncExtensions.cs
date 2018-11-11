using Polib.Net.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace Polib.Net.Synchronization
{
    /// <summary>
    /// Provides extension methods for synchronizing catalogs and translations.
    /// </summary>
    public static class SyncExtensions
    {
        /// <summary>
        /// Detects and returns all changes relative to the specified <paramref name="unchangedCollection"/>.
        /// </summary>
        /// <param name="potentialChanges">The potentially changed collection.</param>
        /// <param name="unchangedCollection">The original, unchanged collection of translations.</param>
        /// <returns></returns>
        public static bool MergeChanges(this IList<UpdatedTranslation> potentialChanges, IList<ITranslation> unchangedCollection)
        {
            if (unchangedCollection.Count != potentialChanges.Count) return false;

            var hasChanges = false;

            for (int i = 0; i < potentialChanges.Count; i++)
            {
                var maybeChanged = potentialChanges[i];
                var unchanged = unchangedCollection[i];

                if (unchanged.Key != maybeChanged.Key)
                {
                    // the keys must match in the same order
                    return false;
                }

                // make sure to use the '|' operator instead of '||' 
                // because Sync is supposed to modify the 'unchanged'
                if (Sync(unchanged.Translations, maybeChanged.Translations) |
                    Sync(unchanged.Flags, maybeChanged.Flags) |
                    Sync(unchanged.References, maybeChanged.References) |
                    Sync(unchanged, maybeChanged))
                {
                    hasChanges = true;
                }
            }

            return hasChanges;
        }

        /// <summary>
        /// Saves the changes made to the specified catalog on disk.
        /// </summary>
        /// <param name="catalog">The catalog to save.</param>
        /// <param name="backup">Whether to create a backup of the original catalog.</param>
        /// <param name="wrapReferences">Whether to word-wrap comment references.</param>
        /// <returns></returns>
        public static bool SaveChanges(this ICatalog catalog, bool backup = false, bool wrapReferences = false)
        {
            try
            {
                // save the changes to a temporary file location
                var tempname = new PoFileWriter(catalog) { WordWrapReferences = wrapReferences }.SaveChanges();

                var catalogName = catalog.FileName;

                if (File.Exists(catalogName))
                {
                    if (backup)
                    {
                        File.Move(catalogName, $"{catalogName}.{DateTime.Now.ToString("yyyy-MM-dd-HHmmss")}.bak");
                    }
                    else
                    {
                        File.Delete(catalogName);
                    }
                }

                File.Copy(tempname, catalogName);
                return true;
            }
            catch //(Exception ex)
            {
                //System.Diagnostics.Trace.WriteLine(ex);
            }

            return false;
        }

        #region private helpers

        /// <summary>
        /// Compares <paramref name="unchanged"/> to <paramref name="maybeChanged"/>, and 
        /// if differences exist, synchronizes them with the <paramref name="unchanged"/>.
        /// </summary>
        /// <param name="unchanged">The unchanged collection.</param>
        /// <param name="maybeChanged">The collection containing potential changes.</param>
        /// <returns></returns>
        static bool Sync(IList<string> unchanged, IList<string> maybeChanged)
        {
            var needsUpdate = false;

            for (int i = 0; i < maybeChanged.Count && i < unchanged.Count; i++)
            {
                if (unchanged[i] != maybeChanged[i])
                {
                    unchanged[i] = maybeChanged[i];
                    needsUpdate = true;
                }
            }

            return needsUpdate;
        }

        static bool Sync(ITranslation unchanged, IUpdatedTranslation maybeChanged)
        {
            var changed = false;

            if (!string.Equals(unchanged.Context, maybeChanged.Context))
            {
                unchanged.Context = maybeChanged.Context;
                changed = true;
            }
            if (!string.Equals(unchanged.ExtractedComments, maybeChanged.ExtractedComments))
            {
                unchanged.ExtractedComments = maybeChanged.ExtractedComments;
                changed = true;
            }
            if (!string.Equals(unchanged.Plural, maybeChanged.Plural))
            {
                unchanged.Plural = maybeChanged.Plural;
                changed = true;
            }
            if (!string.Equals(unchanged.Singular, maybeChanged.Singular))
            {
                unchanged.Singular = maybeChanged.Singular;
                changed = true;
            }
            if (!string.Equals(unchanged.TranslatorComments, maybeChanged.TranslatorComments))
            {
                unchanged.TranslatorComments = maybeChanged.TranslatorComments;
                changed = true;
            }

            return changed;
        } 

        #endregion
    }
}
