using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Polib.Net
{
    /// <summary>
    /// Represents a list of <see cref="TranslationEntry"/> items.
    /// </summary>
    public class TranslationEntryList : IList<TranslationEntry>
    {
        #region fields

        private readonly List<TranslationEntry> _list;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationEntryList"/> class.
        /// </summary>
        public TranslationEntryList()
        {
            _list = new List<TranslationEntry>();
        }

        #endregion

        #region public instance methods

        /// <summary>
        /// Returns the first translation identified by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The message identifier of the translation to find.</param>
        /// <returns></returns>
        public virtual TranslationEntry FindSingular(string id) => _list.Where(i => i.Singular == id).FirstOrDefault();

        /// <summary>
        /// Returns the first plural form of the translation identified by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The plural message identifier of the translation to find.</param>
        /// <returns></returns>
        public virtual TranslationEntry FindPlural(string id) => _list.Where(i => i.Plural == id).FirstOrDefault();

        /// <summary>
        /// Returns the first contextual translation identified by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The message identifier of the translation to find.</param>
        /// <param name="context">The context to find.</param>
        /// <returns></returns>
        public virtual TranslationEntry FindContext(string id, string context) 
            => _list.Where(i => i.Singular == id && i.Context == context).FirstOrDefault();

        #endregion

        #region IList<PoTranslation> Interface Implementation

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public TranslationEntry this[int index] { get => _list[index]; set => _list[index] = value; }

        /// <summary>
        /// Returns the number of elements contained in the list.
        /// </summary>
        public int Count => _list.Count;

        /// <summary>
        /// Returns false.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds an element to the end of the list.
        /// </summary>
        /// <param name="item"></param>
        public void Add(TranslationEntry item) => _list.Add(item);

        /// <summary>
        /// Removes all elements from the list.
        /// </summary>
        public void Clear() => _list.Clear();

        /// <summary>
        /// Determines whether an element is in the list.
        /// </summary>
        /// <param name="item">The element to locate in the list.</param>
        /// <returns>true if item is found in the list; otherwise, false.</returns>
        public bool Contains(TranslationEntry item) => _list.Contains(item);

        /// <summary>
        /// Copies the entire list to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements copied from list. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex"> The zero-based index in array at which copying begins.</param>
        public void CopyTo(TranslationEntry[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        /// <summary>
        /// Returns an enumerator that iterates through the list.
        /// </summary>
        /// <returns>A <see cref="List{T}.Enumerator"/> for the list.</returns>
        public IEnumerator<TranslationEntry> GetEnumerator() => _list.GetEnumerator();

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the entire list.
        /// </summary>
        /// <param name="item">The object to locate in the list. The value can be null for reference types.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire list, if found; otherwise, –1.</returns>
        public int IndexOf(TranslationEntry item) => _list.IndexOf(item);

        /// <summary>
        /// Inserts an element into the list at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert. The value can be null for reference types.</param>
        public void Insert(int index, TranslationEntry item) => _list.Insert(index, item);

        /// <summary>
        ///  Removes the first occurrence of a specific object from the list.
        /// </summary>
        /// <param name="item">The object to remove from the list. The value can be null for reference types.</param>
        /// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the list.</returns>
        public bool Remove(TranslationEntry item) => _list.Remove(item);

        /// <summary>
        /// Removes the element at the specified index of the list.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public void RemoveAt(int index) => _list.RemoveAt(index);

        /// <summary>
        /// Returns an enumerator that iterates through the list.
        /// </summary>
        /// <returns>A <see cref="List{T}.Enumerator"/> for the list.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
