using System;
using System.Collections.Generic;
using System.Linq;
using DataStructure;
using Xunit;

namespace DataStructureTests
{
    public class DynamicArrayTests
    {
        #region Constructor and Basic Properties
        [Fact]
        public void Constructor_WithCapacity_CreatesEmptyArray()
        {
            var array = new DynamicArray<int>(10);
            Assert.Equal(0, array.Count);
            Assert.Equal(10, array.Capacity);
        }

        [Fact]
        public void Constructor_WithCollection_CopiesElements()
        {
            var collection = new List<int> { 1, 2, 3 };
            var array = new DynamicArray<int>(collection);

            Assert.Equal(3, array.Count);
            Assert.Equal(1, array[0]);
            Assert.Equal(2, array[1]);
            Assert.Equal(3, array[2]);
        }

        [Fact]
        public void Constructor_InvalidCapacity_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DynamicArray<int>(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DynamicArray<int>(-1));
        }
        #endregion

        #region Add Operations
        [Fact]
        public void Add_IncreasesCount()
        {
            var array = new DynamicArray<int>();
            array.Add(10);
            array.Add(20);

            Assert.Equal(2, array.Count);
            Assert.Equal(10, array[0]);
            Assert.Equal(20, array[1]);
        }

        [Fact]
        public void AddRange_WithEnumerable_AddsAllElements()
        {
            var array = new DynamicArray<int>();
            array.AddRange(Enumerable.Range(1, 100));

            Assert.Equal(100, array.Count);
            Assert.Equal(1, array[0]);
            Assert.Equal(100, array[99]);
        }

        [Fact]
        public void AddRange_WithSpan_AddsAllElements()
        {
            var array = new DynamicArray<int>();
            var span = new ReadOnlySpan<int>(new[] { 10, 20, 30 });
            array.AddRange(span);

            Assert.Equal(3, array.Count);
            Assert.Equal(10, array[0]);
            Assert.Equal(30, array[2]);
        }
        #endregion

        #region Insert Operations
        [Fact]
        public void Insert_AtBeginning_ShiftsElements()
        {
            var array = new DynamicArray<int>(new[] { 2, 3 });
            array.Insert(0, 1);

            Assert.Equal(3, array.Count);
            Assert.Equal(1, array[0]);
            Assert.Equal(2, array[1]);
            Assert.Equal(3, array[2]);
        }

        [Fact]
        public void Insert_AtEnd_AddsElement()
        {
            var array = new DynamicArray<int>(new[] { 1, 2 });
            array.Insert(2, 3);

            Assert.Equal(3, array.Count);
            Assert.Equal(3, array[2]);
        }

        [Fact]
        public void InsertRange_WithCollection_InsertsElements()
        {
            var array = new DynamicArray<int>(new[] { 1, 4 });
            array.InsertRange(1, new[] { 2, 3 });

            Assert.Equal(4, array.Count);
            Assert.Equal(1, array[0]);
            Assert.Equal(2, array[1]);
            Assert.Equal(3, array[2]);
            Assert.Equal(4, array[3]);
        }
        #endregion

        #region Remove Operations
        [Fact]
        public void RemoveAt_DecreasesCount()
        {
            var array = new DynamicArray<int>(new[] { 1, 2, 3 });
            array.RemoveAt(1);

            Assert.Equal(2, array.Count);
            Assert.Equal(1, array[0]);
            Assert.Equal(3, array[1]);
        }

        [Fact]
        public void Remove_ExistingItem_ReturnsTrue()
        {
            var array = new DynamicArray<string>(new[] { "a", "b", "c" });
            var result = array.Remove("b");

            Assert.True(result);
            Assert.Equal(2, array.Count);
            Assert.DoesNotContain("b", array);
        }

        [Fact]
        public void Remove_NonExistingItem_ReturnsFalse()
        {
            var array = new DynamicArray<string>(new[] { "a", "b", "c" });
            var result = array.Remove("d");

            Assert.False(result);
            Assert.Equal(3, array.Count);
        }

        [Fact]
        public void RemoveLast_ReturnsLastElement()
        {
            var array = new DynamicArray<int>(new[] { 1, 2, 3 });
            var last = array.RemoveLast();

            Assert.Equal(3, last);
            Assert.Equal(2, array.Count);
            Assert.Equal(2, array[1]);
        }

 
        [Fact]
        public void RemoveRange_RemovesElements()
        {
            var array = new DynamicArray<int>(Enumerable.Range(1, 10));
            array.RemoveRange(2, 4);   // Remove 4 elements starting at index 2

            Assert.Equal(6, array.Count);
            Assert.Equal(1, array[0]);
            Assert.Equal(2, array[1]);
            Assert.Equal(7, array[2]);  // Original index 6 moves to index 2
            Assert.Equal(8, array[3]);
            Assert.Equal(9, array[4]);
            Assert.Equal(10, array[5]);
        }
        [Fact]
        public void RemoveRange_RemovesInclusiveRange()
        {
            var array = new DynamicArray<int>(new[] { 1, 2, 3, 4, 5 });
            array.RemoveRange(1, 3);   // Remove indices 1, 2, 3

            Assert.Equal(2, array.Count);
            Assert.Equal(1, array[0]);
            Assert.Equal(5, array[1]);
        }
        [Fact]
        public void RemoveAll_WithPredicate_RemovesMatchingElements()
        {
            var array = new DynamicArray<int>(Enumerable.Range(1, 10));
            var removed = array.RemoveAll(x => x % 2 == 0);

            Assert.Equal(5, removed);
            Assert.Equal(5, array.Count);
            Assert.All(array, x => Assert.True(x % 2 != 0));
        }

        [Fact]
        public void Clear_ResetsCount()
        {
            var array = new DynamicArray<int>(Enumerable.Range(1, 10));
            array.Clear();

            Assert.Equal(0, array.Count);
            Assert.True(array.Capacity > 0);
        }
        #endregion

        #region Mathematical Operations
        [Fact]
        public void Sum_CalculatesCorrectValue()
        {
            var array = new DynamicArray<int>(new[] { 1, 2, 3, 4 });
            Assert.Equal(10, array.Sum());
        }

        [Fact]
        public void Average_CalculatesCorrectValue()
        {
            var array = new DynamicArray<double>(new[] { 1.0, 2.0, 3.0, 4.0 });
            Assert.Equal(2.5, array.Average());
        }

        [Fact]
        public void Min_ReturnsSmallestValue()
        {
            var array = new DynamicArray<int>(new[] { 5, 2, 8, 1 });
            Assert.Equal(1, array.Min());
        }

        [Fact]
        public void Max_ReturnsLargestValue()
        {
            var array = new DynamicArray<int>(new[] { 5, 2, 8, 1 });
            Assert.Equal(8, array.Max());
        }

        [Fact]
        public void Median_ReturnsMiddleValue()
        {
            var array = new DynamicArray<int>(new[] { 5, 2, 8, 1, 3 });
            Assert.Equal(3, array.Median());
        }
        #endregion

        #region Sorting and Searching
        [Fact]
        public void Sort_OrdersElements()
        {
            var array = new DynamicArray<int>(new[] { 5, 2, 8, 1 });
            array.Sort();

            Assert.Equal(1, array[0]);
            Assert.Equal(2, array[1]);
            Assert.Equal(5, array[2]);
            Assert.Equal(8, array[3]);
        }

        [Fact]
        public void IndexOf_FindsElement()
        {
            var array = new DynamicArray<string>(new[] { "a", "b", "c" });
            Assert.Equal(1, array.IndexOf("b"));
        }

        [Fact]
        public void Contains_ReturnsCorrectValue()
        {
            var array = new DynamicArray<string>(new[] { "a", "b", "c" });
            Assert.True(array.Contains("b"));
            Assert.False(array.Contains("d"));
        }

        [Fact]
        public void BinarySearch_FindsElementInSortedArray()
        {
            var array = new DynamicArray<int>(new[] { 1, 2, 3, 4, 5 });
            var index = array.BinarySearch(3);

            Assert.Equal(2, index);
        }

        [Fact]
        public void Find_ReturnsFirstMatchingElement()
        {
            var array = new DynamicArray<int>(new[] { 1, 2, 3, 4, 5 });
            var result = array.Find(x => x > 3);

            Assert.Equal(4, result);
        }

        [Fact]
        public void FindAll_ReturnsAllMatchingElements()
        {
            var array = new DynamicArray<int>(new[] { 1, 2, 3, 4, 5 });
            var result = array.FindAll(x => x % 2 == 0);

            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0]);
            Assert.Equal(4, result[1]);
        }
        #endregion

        #region Capacity Management
        [Fact]
        public void EnsureCapacity_ExpandsWhenNeeded()
        {
            var array = new DynamicArray<int>(4);
            array.EnsureCapacity(100);

            Assert.True(array.Capacity >= 100);
        }

        [Fact]
        public void Compact_ReducesCapacityToCount()
        {
            var array = new DynamicArray<int>(100);
            array.AddRange(Enumerable.Range(1, 10));
            array.Compact();

            Assert.Equal(10, array.Capacity);
        }

        [Fact]
        public void TrimExcess_ReducesCapacityWhenUnderThreshold()
        {
            var array = new DynamicArray<int>(100);
            array.AddRange(Enumerable.Range(1, 10));
            array.TrimExcess(0.5); // 50% threshold

            Assert.True(array.Capacity < 100);
        }

        [Fact]
        public void Resize_ChangesCount()
        {
            var array = new DynamicArray<int>(new[] { 1, 2, 3, 4, 5 });
            array.Resize(3);

            Assert.Equal(3, array.Count);
            Assert.Equal(1, array[0]);
            Assert.Equal(2, array[1]);
            Assert.Equal(3, array[2]);
        }
        #endregion

        #region Advanced Operations
        [Fact]
        public void Rotate_ShiftsElements()
        {
            var array = new DynamicArray<int>(new[] { 1, 2, 3, 4, 5 });
            array.Rotate(2);

            Assert.Equal(4, array[0]);
            Assert.Equal(5, array[1]);
            Assert.Equal(1, array[2]);
            Assert.Equal(2, array[3]);
            Assert.Equal(3, array[4]);
        }

        [Fact]
        public void Flip_ReversesElements()
        {
            var array = new DynamicArray<int>(new[] { 1, 2, 3, 4 });
            array.Flip();

            Assert.Equal(4, array[0]);
            Assert.Equal(3, array[1]);
            Assert.Equal(2, array[2]);
            Assert.Equal(1, array[3]);
        }

        [Fact]
        public void Shuffle_RandomizesOrder()
        {
            var array = new DynamicArray<int>(Enumerable.Range(1, 100));
            var originalOrder = array.ToArray();
            array.Shuffle();

            Assert.Equal(100, array.Count);
            Assert.NotEqual(originalOrder, array.ToArray());
            Assert.Equal(originalOrder.OrderBy(x => x), array.ToArray().OrderBy(x => x));
        }

        [Fact]
        public void Distinct_ReturnsUniqueElements()
        {
            var array = new DynamicArray<int>(new[] { 1, 2, 2, 3, 3, 3 });
            var distinct = array.Distinct();

            Assert.Equal(3, distinct.Count);
            Assert.Equal(1, distinct[0]);
            Assert.Equal(2, distinct[1]);
            Assert.Equal(3, distinct[2]);
        }

        [Fact]
        public void AddIfNotContains_AddsOnlyNewElements()
        {
            var array = new DynamicArray<int>(new[] { 1, 2, 3 });
            var added1 = array.AddIfNotContains(3);
            var added2 = array.AddIfNotContains(4);

            Assert.False(added1);
            Assert.True(added2);
            Assert.Equal(4, array.Count);
        }

        [Fact]
        public void ConvertAll_ConvertsTypes()
        {
            var intArray = new DynamicArray<int>(new[] { 1, 2, 3 });
            var stringArray = intArray.ConvertAll(x => x.ToString());

            Assert.Equal("1", stringArray[0]);
            Assert.Equal("2", stringArray[1]);
            Assert.Equal("3", stringArray[2]);
        }

        [Fact]
        public void Move_ChangesElementPosition()
        {
            var array = new DynamicArray<int>(new[] { 1, 2, 3, 4 });
            array.Move(1, 3);  // Move element 2 from position 1 to position 3

            Assert.Equal(1, array[0]);
            Assert.Equal(3, array[1]);
            Assert.Equal(4, array[2]);  // Now correctly positioned
            Assert.Equal(2, array[3]);
        }
        [Fact]
        public void Move_Backward_ShiftsElements()
        {
            var array = new DynamicArray<int>(new[] { 1, 2, 3, 4 });
            array.Move(3, 0);  // Move last element to front

            Assert.Equal(4, array[0]);
            Assert.Equal(1, array[1]);
            Assert.Equal(2, array[2]);
            Assert.Equal(3, array[3]);
        }

        [Fact]
        public void Move_ToSamePosition_NoChange()
        {
            var array = new DynamicArray<int>(new[] { 1, 2, 3 });
            array.Move(1, 1);

            Assert.Equal(1, array[0]);
            Assert.Equal(2, array[1]);
            Assert.Equal(3, array[2]);
        }

        [Fact]

        public void Move_AdjacentForward()
        {
            var array = new DynamicArray<int>(new[] { 1, 2, 3 });
            array.Move(0, 1);  // Move first element to second position

            Assert.Equal(2, array[0]);  // First element becomes 2
            Assert.Equal(1, array[1]);  // Moved element at second position
            Assert.Equal(3, array[2]);  // Last element remains
        }
        [Fact]
        public void Move_ToEndPosition()
        {
            var array = new DynamicArray<int>(new[] { 1, 2, 3, 4 });
            array.Move(1, 3);  // Move second element to end

            Assert.Equal(1, array[0]);
            Assert.Equal(3, array[1]);
            Assert.Equal(4, array[2]);
            Assert.Equal(2, array[3]);  // Moved element at end
        }

        [Fact]
        public void Move_FromEndToStart()
        {
            var array = new DynamicArray<int>(new[] { 1, 2, 3, 4 });
            array.Move(3, 0);  // Move last element to start

            Assert.Equal(4, array[0]);  // Moved element first
            Assert.Equal(1, array[1]);
            Assert.Equal(2, array[2]);
            Assert.Equal(3, array[3]);
        }

        [Fact]
        public void Move_WithinRange()
        {
            var array = new DynamicArray<int>(Enumerable.Range(1, 10));
            array.Move(3, 7);  // Move 4th element to 8th position

            Assert.Equal(1, array[0]);
            Assert.Equal(2, array[1]);
            Assert.Equal(3, array[2]);
            Assert.Equal(5, array[3]);  // Elements shift left after removal
            Assert.Equal(6, array[4]);
            Assert.Equal(7, array[5]);
            Assert.Equal(8, array[6]);
            Assert.Equal(4, array[7]);  // Moved element at new position
            Assert.Equal(9, array[8]);
            Assert.Equal(10, array[9]);
        }
        [Fact]
        public void Replace_ChangesMatchingElements()
        {
            var array = new DynamicArray<string>(new[] { "a", "b", "a", "c" });
            array.Replace("a", "x");

            Assert.Equal("x", array[0]);
            Assert.Equal("b", array[1]);
            Assert.Equal("x", array[2]);
            Assert.Equal("c", array[3]);
        }
        #endregion

        #region String Extensions
        [Fact]
        public void ToLower_ConvertsAllStrings()
        {
            var array = new DynamicArray<string>(new[] { "ABC", "DeF", "gHi" });
            array.ToLower();

            Assert.Equal("abc", array[0]);
            Assert.Equal("def", array[1]);
            Assert.Equal("ghi", array[2]);
        }

        [Fact]
        public void ToUpper_ConvertsAllStrings()
        {
            var array = new DynamicArray<string>(new[] { "abc", "DeF", "gHi" });
            array.ToUpper();

            Assert.Equal("ABC", array[0]);
            Assert.Equal("DEF", array[1]);
            Assert.Equal("GHI", array[2]);
        }

        [Fact]
        public void ChangeCaseRange_ConvertsSpecificRange()
        {
            var array = new DynamicArray<string>(new[] { "a", "b", "c", "d" });
            array.ChangeCaseRange(1, 2, true); // Convert index 1-2 to uppercase

            Assert.Equal("a", array[0]);
            Assert.Equal("B", array[1]);
            Assert.Equal("C", array[2]);
            Assert.Equal("d", array[3]);
        }
        #endregion

        #region Factory Methods
        [Fact]
        public void CreateRandom_GeneratesNumericArray()
        {
            var array = DynamicArray<double>.CreateRandom(100, 0, 100);
            Assert.Equal(100, array.Count);
            Assert.All(array, x =>
            {
                Assert.True(x >= 0);
                Assert.True(x < 100);
            });
        }

        [Fact]
        public void CreateRandomStrings_GeneratesStringArray()
        {
            var array = DynamicArray<string>.CreateRandomStrings(50, 5, 10);
            Assert.Equal(50, array.Count);
            Assert.All(array, s =>
            {
                Assert.True(s.Length >= 5 && s.Length <= 10);
                Assert.True(s.All(char.IsLetter));
            });
        }
        #endregion

        #region Edge Cases
        [Fact]
        public void EmptyArray_OperationsThrowProperly()
        {
            var array = new DynamicArray<int>();

            Assert.Throws<InvalidOperationException>(() => array.Average());
            Assert.Throws<InvalidOperationException>(() => array.Median());
            Assert.Throws<InvalidOperationException>(() => array.Min());
            Assert.Throws<InvalidOperationException>(() => array.Max());
            Assert.Throws<InvalidOperationException>(() => array.RemoveLast());
        }

        [Fact]
        public void Indexer_OutOfRange_ThrowsException()
        {
            var array = new DynamicArray<int>(new[] { 1, 2, 3 });

            Assert.Throws<ArgumentOutOfRangeException>(() => array[-1]);
            Assert.Throws<ArgumentOutOfRangeException>(() => array[3]);
            Assert.Throws<ArgumentOutOfRangeException>(() => array[100]);
        }
        #endregion
    }
}
