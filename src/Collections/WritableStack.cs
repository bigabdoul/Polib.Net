using System.Collections.Generic;

namespace Polib.Net.Collections
{
    /// <summary>
    /// Represents a stack whose top item can be changed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class WritableStack<T>
    {
        readonly IList<T> InternalList = new List<T>();

        /// <summary>
        /// Initializes a new instance of the <see cref="WritableStack{T}"/>.
        /// </summary>
        internal WritableStack()
        {
        }

        /// <summary>
        /// Gets the number of items contained in the internal list.
        /// </summary>
        internal int Count { get => InternalList.Count; }

        /// <summary>
        /// Inserts the specified item at the top of the stack.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        internal void Push(T item)
        {
            InternalList.Insert(0, item);
        }

        /// <summary>
        /// Returns the top item from the stack without removing it.
        /// </summary>
        /// <returns></returns>
        internal T Peek() => InternalList[0];

        /// <summary>
        /// Removes the top item from the stack and returns it.
        /// </summary>
        /// <returns></returns>
        internal T Pop()
        {
            var item = InternalList[0];
            InternalList.RemoveAt(0);
            return item;
        }

        /// <summary>
        /// Changes the top item on the stack.
        /// </summary>
        /// <param name="item">The item to set.</param>
        internal void Set(T item) => InternalList[0] = item;
    }
}
