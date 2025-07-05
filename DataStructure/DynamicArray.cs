using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataStructure
{
    /// <summary>
    /// Dynamic array with advanced manipulation, search and capacity management features.
    /// Supports operations similar to List<T> with additional optimizations.
    /// </summary>
    /// <typeparam name="T">Element type in the array</typeparam>
    public class DynamicArray<T> : IEnumerable<T>
    {
        #region Constants and internal fields
        private const int GrowthFactor = 2;           // Growth factor when expanding
        private const double ShrinkThreshold = 0.25;  // Threshold for capacity reduction

        private T[] _buffer;                         // Internal storage buffer
        #endregion

        #region Public properties
        /// <summary>Current array capacity</summary>
        public int Capacity { get; private set; }

        /// <summary>Actual number of elements present</summary>
        public int Count { get; private set; }
        #endregion

        #region Constructors and initialization
        /// <summary>Creates a new dynamic array with initial capacity</summary>
        /// <param name="capacity">Initial capacity (default: 4)</param>
        /// <exception cref="ArgumentOutOfRangeException">If capacity is ≤ 0</exception>
        public DynamicArray(int capacity = 4)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _buffer = new T[capacity];
            Capacity = capacity;
        }

        /// <summary>Creates a new dynamic array from an existing collection</summary>
        /// <param name="collection">Collection of elements to add</param>
        public DynamicArray(IEnumerable<T> collection) : this()
        {
            AddRange(collection);
        }
        #endregion

        #region Factory methods for random creation
        /// <summary>
        /// Creates a new array with random numeric elements
        /// </summary>
        /// <param name="count">Number of elements to generate</param>
        /// <param name="minValue">Minimum value (inclusive)</param>
        /// <param name="maxValue">Maximum value (exclusive)</param>
        /// <param name="rng">Random number generator (optional)</param>
        /// <exception cref="InvalidOperationException">If type is not supported</exception>
        public static DynamicArray<T> CreateRandom(
            int count,
            double minValue = 0,
            double maxValue = 100,
            Random rng = null)
        {
            if (!IsNumericType(typeof(T)))
                throw new InvalidOperationException("Non-numeric type not supported");

            rng ??= new Random();
            var array = new DynamicArray<T>(count);

            for (int i = 0; i < count; i++)
            {
                double randomValue = minValue + (rng.NextDouble() * (maxValue - minValue));
                array.Add((T)Convert.ChangeType(randomValue, typeof(T)));
            }

            return array;
        }

        /// <summary>
        /// Creates a new array with random strings (only for T = string)
        /// </summary>
        /// <param name="count">Number of strings to generate</param>
        /// <param name="minLength">Minimum string length (default: 1)</param>
        /// <param name="maxLength">Maximum string length (default: 20)</param>
        /// <param name="rng">Random number generator (optional)</param>
        /// <exception cref="InvalidOperationException">If T is not string</exception>
        public static DynamicArray<T> CreateRandomStrings(
            int count,
            int minLength = 1,
            int maxLength = 20,
            Random rng = null)
        {
            if (typeof(T) != typeof(string))
                throw new InvalidOperationException("Method valid only for strings");

            rng ??= new Random();
            var array = new DynamicArray<T>(count);
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

            for (int i = 0; i < count; i++)
            {
                int length = rng.Next(minLength, maxLength + 1);
                string randomString = new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[rng.Next(s.Length)]).ToArray());

                array.Add((T)(object)randomString);
            }

            return array;
        }
        #endregion

        #region Add operations
        /// <summary>Adds an element to the end of the array</summary>
        /// <param name="value">Element to add</param>
        public void Add(T value)
        {
            EnsureSpace(1);
            _buffer[Count++] = value;
        }

        /// <summary>Adds a sequence of elements from a span</summary>
        /// <param name="values">Elements to add</param>
        public void AddRange(ReadOnlySpan<T> values)
        {
            if (values.IsEmpty) return;

            EnsureSpace(values.Length);
            values.CopyTo(_buffer.AsSpan(Count));
            Count += values.Length;
        }

        /// <summary>Adds a sequence of elements from an enumerable</summary>
        /// <param name="values">Elements to add</param>
        /// <exception cref="ArgumentNullException">If values is null</exception>
        public void AddRange(IEnumerable<T> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            // Optimization for collections with known size
            if (values is ICollection<T> collection)
            {
                EnsureSpace(collection.Count);
                collection.CopyTo(_buffer, Count);
                Count += collection.Count;
            }
            else
            {
                foreach (var item in values)
                    Add(item);
            }
        }
        public void AddRange(T[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            AddRange(new ReadOnlySpan<T>(values));
        }
        #endregion

        #region Insert operations
        /// <summary>Inserts an element at the specified position</summary>
        /// <param name="index">Insertion index</param>
        /// <param name="value">Element to insert</param>
        /// <exception cref="ArgumentOutOfRangeException">If index is out of range</exception>
        public void Insert(int index, T value)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            EnsureSpace(1);

            // Shift elements to make space
            if (index < Count)
                _buffer.AsSpan(index, Count - index).CopyTo(_buffer.AsSpan(index + 1));

            _buffer[index] = value;
            Count++;
        }

        /// <summary>Inserts a sequence of elements at the specified position</summary>
        /// <param name="index">Starting index</param>
        /// <param name="items">Elements to insert</param>
        /// <exception cref="ArgumentNullException">If items is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">If index is out of range</exception>
        public void InsertRange(int index, IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            // Optimization for collections with known size
            if (items is ICollection<T> collection)
            {
                int newItemsCount = collection.Count;
                EnsureSpace(newItemsCount);

                // Shift existing elements
                if (Count > index)
                    Array.Copy(_buffer, index, _buffer, index + newItemsCount, Count - index);

                collection.CopyTo(_buffer, index);
                Count += newItemsCount;
            }
            else
            {
                foreach (var item in items)
                    Insert(index++, item);
            }
        }
        #endregion

        #region Remove operations
        /// <summary>Removes the element at the specified position</summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException">If index is out of range</exception>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            // Shift subsequent elements
            if (index < Count - 1)
                _buffer.AsSpan(index + 1, Count - index - 1).CopyTo(_buffer.AsSpan(index));

            Count--;

            // Clear reference to prevent memory leak
            if (!typeof(T).IsValueType)
                _buffer[Count] = default;

            MaybeShrink();
        }

        /// <summary>Removes the first occurrence of the specified element</summary>
        /// <param name="item">Element to remove</param>
        /// <returns>True if found and removed, otherwise false</returns>
        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <summary>Removes and returns the last element</summary>
        /// <returns>Removed last element</returns>
        /// <exception cref="InvalidOperationException">If array is empty</exception>
        public T RemoveLast()
        {
            if (Count == 0)
                throw new InvalidOperationException("Empty array");

            T last = _buffer[Count - 1];
            RemoveAt(Count - 1);
            return last;
        }

        /// <summary>Removes a range of elements</summary>
        /// <param name="startIndex">Start index</param>
        /// <param name="endIndex">End index (inclusive)</param>
        /// <exception cref="ArgumentOutOfRangeException">If indices are invalid</exception>
        public void RemoveRange(int startIndex, int count)
        {
            if (startIndex < 0 || startIndex >= Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count < 0 || startIndex + count > Count)
                throw new ArgumentOutOfRangeException(nameof(count));

            int endIndex = startIndex + count - 1;
            int removeCount = endIndex - startIndex + 1;
            int tailCount = Count - endIndex - 1;

            if (tailCount > 0)
                _buffer.AsSpan(endIndex + 1, tailCount).CopyTo(_buffer.AsSpan(startIndex));

            Count -= removeCount;

            if (!typeof(T).IsValueType)
                Array.Clear(_buffer, Count, removeCount);

            MaybeShrink();
        }

        /// <summary>Removes all elements that satisfy a condition</summary>
        /// <param name="match">Predicate to filter elements</param>
        /// <returns>Number of removed elements</returns>
        /// <exception cref="ArgumentNullException">If match is null</exception>
        public int RemoveAll(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            int freeIndex = 0;
            // Filter elements to keep
            for (int i = 0; i < Count; i++)
            {
                if (!match(_buffer[i]))
                {
                    _buffer[freeIndex++] = _buffer[i];
                }
            }

            int removed = Count - freeIndex;
            if (removed > 0)
            {
                // Clear removed elements
                if (!typeof(T).IsValueType)
                    Array.Clear(_buffer, freeIndex, removed);

                Count = freeIndex;
                MaybeShrink();
            }
            return removed;
        }

        /// <summary>Completely clears the array</summary>
        public void Clear()
        {
            if (!typeof(T).IsValueType)
                Array.Clear(_buffer, 0, Count);

            Count = 0;
        }
        #endregion

        #region Mathematical operations
        /// <summary>Calculates the sum of elements</summary>
        /// <returns>Sum converted to double</returns>
        public double Sum()
        {
            double acc = 0;
            for (int i = 0; i < Count; i++)
                acc += Convert.ToDouble(_buffer[i]);
            return acc;
        }

        /// <summary>Calculates the average of elements</summary>
        /// <returns>Average of elements</returns>
        /// <exception cref="InvalidOperationException">If array is empty</exception>
        public double Average()
        {
            if (Count == 0)
                throw new InvalidOperationException("Empty array.");

            return Sum() / Count;
        }

        /// <summary>Finds the minimum element</summary>
        /// <param name="comparer">Custom comparer (optional)</param>
        /// <returns>Minimum element</returns>
        /// <exception cref="InvalidOperationException">If array is empty</exception>
        public T Min(IComparer<T> comparer = null)
        {
            if (Count == 0)
                throw new InvalidOperationException("Empty array.");

            comparer ??= Comparer<T>.Default;
            T min = _buffer[0];

            for (int i = 1; i < Count; i++)
                if (comparer.Compare(_buffer[i], min) < 0)
                    min = _buffer[i];

            return min;
        }

        /// <summary>Finds the maximum element</summary>
        /// <param name="comparer">Custom comparer (optional)</param>
        /// <returns>Maximum element</returns>
        /// <exception cref="InvalidOperationException">If array is empty</exception>
        public T Max(IComparer<T> comparer = null)
        {
            if (Count == 0)
                throw new InvalidOperationException("Empty array.");

            comparer ??= Comparer<T>.Default;
            T max = _buffer[0];

            for (int i = 1; i < Count; i++)
                if (comparer.Compare(_buffer[i], max) > 0)
                    max = _buffer[i];

            return max;
        }

        /// <summary>Calculates the median of elements</summary>
        /// <returns>Median element</returns>
        /// <exception cref="InvalidOperationException">If array is empty</exception>
        public T Median()
        {
            if (Count == 0)
                throw new InvalidOperationException("Empty array.");

            var copy = new T[Count];
            Array.Copy(_buffer, copy, Count);
            Array.Sort(copy);
            return copy[Count / 2];
        }
        #endregion

        #region Sorting and reversing
        /// <summary>Sorts the elements</summary>
        /// <param name="comparer">Custom comparer (optional)</param>
        public void Sort(IComparer<T> comparer = null)
        {
            comparer ??= Comparer<T>.Default;
            Array.Sort(_buffer, 0, Count, comparer);
        }

        /// <summary>Reverses the order of elements</summary>
        public void Reverse() => Reverse(0, Count - 1);
        #endregion

        #region Search and advanced search
        /// <summary>Finds the index of the first occurrence of an element</summary>
        /// <param name="item">Element to search</param>
        /// <param name="start">Starting index</param>
        /// <param name="comparer">Equality comparer (optional)</param>
        /// <returns>Element index or -1 if not found</returns>
        /// <exception cref="ArgumentOutOfRangeException">If start is out of range</exception>
        public int IndexOf(T item, int start = 0, IEqualityComparer<T> comparer = null)
        {
            if (start < 0 || start >= Count)
                throw new ArgumentOutOfRangeException(nameof(start));

            comparer ??= EqualityComparer<T>.Default;

            for (int i = start; i < Count; i++)
                if (comparer.Equals(_buffer[i], item))
                    return i;

            return -1;
        }

        /// <summary>Finds the index of the last occurrence of an element</summary>
        /// <param name="item">Element to search</param>
        /// <param name="comparer">Equality comparer (optional)</param>
        /// <returns>Element index or -1 if not found</returns>
        public int LastIndexOf(T item, IEqualityComparer<T> comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;

            for (int i = Count - 1; i >= 0; i--)
                if (comparer.Equals(_buffer[i], item))
                    return i;

            return -1;
        }

        /// <summary>Checks if an element is present</summary>
        /// <param name="item">Element to search</param>
        /// <returns>True if found, otherwise false</returns>
        public bool Contains(T item) => Contains(item, null);

        /// <summary>Checks if an element is present using a specific comparer</summary>
        /// <param name="item">Element to search</param>
        /// <param name="comparer">Equality comparer (optional)</param>
        /// <returns>True if found, otherwise false</returns>
        public bool Contains(T item, IEqualityComparer<T> comparer)
            => IndexOf(item, 0, comparer) != -1;

        /// <summary>Binary search for a sorted element</summary>
        /// <param name="item">Element to search</param>
        /// <param name="comparer">Sorting comparer (optional)</param>
        /// <returns>Element index or two's complement if not found</returns>
        public int BinarySearch(T item, IComparer<T> comparer = null)
        {
            comparer ??= Comparer<T>.Default;
            return Array.BinarySearch(_buffer, 0, Count, item, comparer);
        }

        /// <summary>Finds the first element that satisfies a condition</summary>
        /// <param name="match">Predicate to filter elements</param>
        /// <returns>Found element or default(T) if not found</returns>
        /// <exception cref="ArgumentNullException">If match is null</exception>
        public T Find(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            for (int i = 0; i < Count; i++)
                if (match(_buffer[i]))
                    return _buffer[i];

            return default;
        }

        /// <summary>Finds all elements that satisfy a condition</summary>
        /// <param name="match">Predicate to filter elements</param>
        /// <returns>New dynamic array with found elements</returns>
        /// <exception cref="ArgumentNullException">If match is null</exception>
        public DynamicArray<T> FindAll(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            var result = new DynamicArray<T>();
            for (int i = 0; i < Count; i++)
                if (match(_buffer[i]))
                    result.Add(_buffer[i]);

            return result;
        }

        /// <summary>Finds the index of the first element that satisfies a condition</summary>
        /// <param name="match">Predicate to filter elements</param>
        /// <param name="startIndex">Starting index</param>
        /// <returns>Element index or -1 if not found</returns>
        /// <exception cref="ArgumentNullException">If match is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">If startIndex is out of range</exception>
        public int FindIndex(Predicate<T> match, int startIndex = 0)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));
            if (startIndex < 0 || startIndex >= Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            for (int i = startIndex; i < Count; i++)
                if (match(_buffer[i]))
                    return i;

            return -1;
        }

        /// <summary>Finds the index of the last element that satisfies a condition</summary>
        /// <param name="match">Predicate to filter elements</param>
        /// <returns>Element index or -1 if not found</returns>
        /// <exception cref="ArgumentNullException">If match is null</exception>
        public int FindLastIndex(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            for (int i = Count - 1; i >= 0; i--)
                if (match(_buffer[i]))
                    return i;

            return -1;
        }
        #endregion

        #region Iteration and enumeration
        /// <summary>Returns an enumerator to iterate through elements</summary>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return _buffer[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Executes an action for each element</summary>
        /// <param name="action">Action to execute</param>
        /// <exception cref="ArgumentNullException">If action is null</exception>
        public void ForEach(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            for (int i = 0; i < Count; i++)
                action(_buffer[i]);
        }
        #endregion

        #region Utilities and miscellaneous operations
        /// <summary>Creates a static array with current elements</summary>
        public T[] ToArray()
        {
            var result = new T[Count];
            Array.Copy(_buffer, result, Count);
            return result;
        }

        /// <summary>Swaps two elements by position</summary>
        /// <param name="i">Index of first element</param>
        /// <param name="j">Index of second element</param>
        /// <exception cref="ArgumentOutOfRangeException">If indices are out of range</exception>
        public void Swap(int i, int j)
        {
            if (i < 0 || i >= Count || j < 0 || j >= Count)
                throw new ArgumentOutOfRangeException();

            (_buffer[i], _buffer[j]) = (_buffer[j], _buffer[i]);
        }

        /// <summary>Shuffles elements randomly</summary>
        /// <param name="rng">Random number generator (optional)</param>
        public void Shuffle(Random rng = null)
        {
            rng ??= new Random();
            for (int i = Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                Swap(i, j);
            }
        }

        /// <summary>Reduces capacity if it exceeds the specified threshold</summary>
        /// <param name="threshold">Usage threshold (default: 90%)</param>
        public void TrimExcess(double threshold = 0.9)
        {
            if (Count < Capacity * threshold)
                ResizeBuffer(Count);
        }

        /// <summary>Creates a shallow copy of the array</summary>
        public DynamicArray<T> Clone()
        {
            var clone = new DynamicArray<T>(Capacity);
            clone.AddRange(_buffer.AsSpan(0, Count));
            return clone;
        }

        /// <summary>Converts all elements to another type</summary>
        /// <typeparam name="TOutput">Output type</typeparam>
        /// <param name="converter">Conversion function</param>
        /// <returns>New converted array</returns>
        /// <exception cref="ArgumentNullException">If converter is null</exception>
        public DynamicArray<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            var result = new DynamicArray<TOutput>(Count);
            for (int i = 0; i < Count; i++)
                result.Add(converter(_buffer[i]));

            return result;
        }

        /// <summary>Moves an element from one position to another</summary>
        /// <param name="fromIndex">Original position</param>
        /// <param name="toIndex">Destination position</param>
        /// <exception cref="ArgumentOutOfRangeException">If indices are out of range</exception>
        public void Move(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= Count)
                throw new ArgumentOutOfRangeException(nameof(fromIndex));
            if (toIndex < 0 || toIndex >= Count)
                throw new ArgumentOutOfRangeException(nameof(toIndex));

            if (fromIndex == toIndex) return;

            T item = _buffer[fromIndex];

            if (toIndex > fromIndex)
            {
                // No adjustment needed - removal happens first
            }

            RemoveAt(fromIndex);
            Insert(toIndex, item);
        }

        /// <summary>Replaces all occurrences of a value</summary>
        /// <param name="oldValue">Value to replace</param>
        /// <param name="newValue">New value</param>
        /// <param name="comparer">Identification comparer (optional)</param>
        public void Replace(T oldValue, T newValue, IEqualityComparer<T> comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;
            for (int i = 0; i < Count; i++)
                if (comparer.Equals(_buffer[i], oldValue))
                    _buffer[i] = newValue;
        }
        #endregion

        #region Capacity management
        /// <summary>Ensures a minimum capacity</summary>
        /// <param name="newCapacity">New minimum capacity</param>
        public void EnsureCapacity(int newCapacity)
        {
            if (newCapacity > Capacity)
                ResizeBuffer(newCapacity);
        }

        /// <summary>Reduces capacity to the actual number of elements</summary>
        public void Compact() => ResizeBuffer(Count);

        /// <summary>Resizes the array while preserving elements</summary>
        /// <param name="newSize">New array size</param>
        /// <exception cref="ArgumentOutOfRangeException">If newSize is negative</exception>
        public void Resize(int newSize)
        {
            if (newSize < 0)
                throw new ArgumentOutOfRangeException(nameof(newSize));

            if (newSize > Capacity)
                ResizeBuffer(newSize);

            // Clear removed elements
            if (newSize < Count && !typeof(T).IsValueType)
                Array.Clear(_buffer, newSize, Count - newSize);

            Count = newSize;
        }
        #endregion

        #region Advanced manipulations
        /// <summary>Rotates elements by a specified number of positions</summary>
        /// <param name="steps">Number of positions (positive: right, negative: left)</param>
        /// <returns>Actual number of steps performed</returns>
        public int Rotate(int steps)
        {
            if (Count == 0) return 0;

            // Normalize steps
            steps %= Count;
            if (steps == 0) return 0;
            if (steps < 0) steps += Count;

            // Efficient rotation algorithm with three reversals
            Reverse(0, Count - 1);
            Reverse(0, steps - 1);
            Reverse(steps, Count - 1);

            return steps;
        }

        /// <summary>Reverses the order of all elements</summary>
        public void Flip() => Flip(0, Count - 1);

        /// <summary>Reverses the order of elements in a range</summary>
        /// <param name="start">Start index</param>
        /// <param name="end">End index (inclusive)</param>
        /// <exception cref="ArgumentOutOfRangeException">If indices are invalid</exception>
        public void Flip(int start, int end)
        {
            if (Count <= 1) return;
            if (start < 0 || end >= Count)
                throw new ArgumentOutOfRangeException();
            if (start > end)
                throw new ArgumentException("start > end");

            Reverse(start, end);
        }
        #endregion

        #region Set operations
        /// <summary>Adds an element only if not already present</summary>
        /// <param name="item">Element to add</param>
        /// <param name="comparer">Equality comparer (optional)</param>
        /// <returns>True if added, false if already present</returns>
        public bool AddIfNotContains(T item, IEqualityComparer<T> comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;
            if (Contains(item, comparer))
                return false;

            Add(item);
            return true;
        }

        /// <summary>Returns unique elements in the array</summary>
        /// <param name="comparer">Equality comparer (optional)</param>
        /// <returns>New array with distinct elements</returns>
        public DynamicArray<T> Distinct(IEqualityComparer<T> comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;
            var result = new DynamicArray<T>();
            var seen = new HashSet<T>(comparer);

            for (int i = 0; i < Count; i++)
                if (seen.Add(_buffer[i]))
                    result.Add(_buffer[i]);

            return result;
        }
        #endregion

        #region Checks and comparisons
        /// <summary>Checks if all elements satisfy a condition</summary>
        /// <param name="match">Predicate to verify</param>
        /// <returns>True if all satisfy the condition</returns>
        /// <exception cref="ArgumentNullException">If match is null</exception>
        public bool TrueForAll(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            for (int i = 0; i < Count; i++)
                if (!match(_buffer[i]))
                    return false;

            return true;
        }

        /// <summary>Compares two arrays for sequential equality</summary>
        /// <param name="other">Array to compare</param>
        /// <param name="comparer">Equality comparer (optional)</param>
        /// <returns>True if arrays are equal</returns>
        /// <exception cref="ArgumentNullException">If other is null</exception>
        public bool SequenceEqual(DynamicArray<T> other, IEqualityComparer<T> comparer = null)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            comparer ??= EqualityComparer<T>.Default;

            if (Count != other.Count)
                return false;

            for (int i = 0; i < Count; i++)
                if (!comparer.Equals(_buffer[i], other[i]))
                    return false;

            return true;
        }
        #endregion

        #region Private helpers
        // Ensures sufficient space for new elements
        private void EnsureSpace(int needed)
        {
            if (Count + needed <= Capacity) return;

            int newCap = Math.Max(Capacity * GrowthFactor, Count + needed);
            ResizeBuffer(newCap);
        }

        // Reduces capacity if below threshold
        private void MaybeShrink()
        {
            if (Capacity <= 4) return;
            if (Count > Capacity * ShrinkThreshold) return;

            int newCap = Math.Max(4, Math.Max(Count, Capacity / GrowthFactor));
            if (newCap < Capacity)
                ResizeBuffer(newCap);
        }

        // Resizes internal buffer
        private void ResizeBuffer(int newCapacity)
        {
            var newBuf = new T[newCapacity];
            _buffer.AsSpan(0, Count).CopyTo(newBuf);
            _buffer = newBuf;
            Capacity = newCapacity;
        }

        // Reverses elements in a range
        private void Reverse(int start, int end)
        {
            while (start < end)
            {
                (_buffer[start], _buffer[end]) = (_buffer[end], _buffer[start]);
                start++;
                end--;
            }
        }

        // Helper to check numeric types
        private static bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region Indexer
        /// <summary>Element access by index</summary>
        /// <param name="index">Element index</param>
        /// <exception cref="ArgumentOutOfRangeException">If index is out of range</exception>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _buffer[index];
            }
            set
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                _buffer[index] = value;
            }
        }
        #endregion
    }
    public static class DinamicArrayStringExtensions
    {
        /// <summary>
        /// Converts all strings in the array to lowercase
        /// </summary>
        public static void ToLower(this DynamicArray<string> array)
        {
            if (array == null) return;

            for (int i = 0; i < array.Count; i++)
            {
                array[i] = array[i]?.ToLowerInvariant();
            }
        }

        /// <summary>
        /// Converts all strings in the array to uppercase
        /// </summary>
        public static void ToUpper(this DynamicArray<string> array)
        {
            if (array == null) return;

            for (int i = 0; i < array.Count; i++)
            {
                array[i] = array[i]?.ToUpperInvariant();
            }
        }

        /// <summary>
        /// Converts strings in a specific range
        /// </summary>
        /// <param name="startIndex">Starting index</param>
        /// <param name="count">Number of elements to convert</param>
        /// <param name="culture">Culture to use (optional)</param>
        /// <param name="toUpper">True for uppercase, false for lowercase</param>
        public static void ChangeCaseRange(
            this DynamicArray<string> array,
            int startIndex,
            int count,
            bool toUpper,
            System.Globalization.CultureInfo culture = null)
        {
            if (array == null) return;
            if (startIndex < 0 || startIndex >= array.Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count < 0 || startIndex + count > array.Count)
                throw new ArgumentOutOfRangeException(nameof(count));

            culture ??= System.Globalization.CultureInfo.CurrentCulture;

            int endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
            {
                if (toUpper)
                    array[i] = array[i]?.ToUpper(culture);
                else
                    array[i] = array[i]?.ToLower(culture);
            }
        }
    }
}