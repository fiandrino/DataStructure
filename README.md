## 🚀 Complexity Analysis

Below is the complete complexity analysis for all operations. The analysis uses Big O notation with:
- **n**: Current element count
- **m**: Elements in input collection
- **k**: Elements affected by operation
- **L**: Average string length

### Core Operations

| Operation                 | Time Complexity        | Space Complexity       | Details |
|---------------------------|------------------------|------------------------|---------|
| **Constructors**          |                        |                        |         |
| `DynamicArray(capacity)`  | O(1)                   | O(capacity)            |         |
| `DynamicArray(collection)`| O(m)                   | O(m)                   |         |
| **Add Operations**        |                        |                        |         |
| `Add(value)`              | O(1) amortized         | O(1) amortized         |         |
| `AddRange(IEnumerable)`   | O(m)                   | O(n+m) amortized       |         |
| `AddRange(ReadOnlySpan)`  | O(m)                   | O(n+m) amortized       |         |
| **Insert Operations**     |                        |                        |         |
| `Insert(index, value)`    | O(n)                   | O(1) amortized         |         |
| `InsertRange(index, items)`| O(n + m)               | O(n+m) amortized       |         |
| **Remove Operations**     |                        |                        |         |
| `RemoveAt(index)`         | O(n)                   | O(1)                   |         |
| `Remove(item)`            | O(n)                   | O(1)                   |         |
| `RemoveLast()`            | O(1)                   | O(1)                   |         |
| `RemoveRange(start, end)` | O(n)                   | O(1)                   |         |
| `RemoveAll(match)`        | O(n)                   | O(1)                   |         |
| `Clear()`                 | O(1) or O(n)           | O(1)                   | *       |

### Mathematical & Sorting Operations

| Operation                 | Time Complexity        | Space Complexity       | Details |
|---------------------------|------------------------|------------------------|---------|
| `Sum()`, `Average()`      | O(n)                   | O(1)                   |         |
| `Min()`, `Max()`          | O(n)                   | O(1)                   |         |
| `Median()`                | O(n log n)             | O(n)                   |         |
| `Sort()`                  | O(n log n)             | O(1) or O(log n)       | **      |
| `Reverse()`               | O(n)                   | O(1)                   |         |

### Search & Utility Operations

| Operation                 | Time Complexity        | Space Complexity       | Details |
|---------------------------|------------------------|------------------------|---------|
| `IndexOf()`               | O(n)                   | O(1)                   |         |
| `LastIndexOf()`           | O(n)                   | O(1)                   |         |
| `Contains()`              | O(n)                   | O(1)                   |         |
| `BinarySearch()`          | O(log n)               | O(1)                   | ***     |
| `Find()`, `FindAll()`     | O(n)                   | O(1) or O(m)           | ****    |
| `ToArray()`               | O(n)                   | O(n)                   |         |
| `Swap(i, j)`              | O(1)                   | O(1)                   |         |
| `Shuffle()`               | O(n)                   | O(1)                   |         |
| `Clone()`                 | O(n)                   | O(n)                   |         |
| `ConvertAll()`            | O(n)                   | O(n)                   |         |
| `Replace()`               | O(n)                   | O(1)                   |         |

### Advanced & Set Operations

| Operation                 | Time Complexity        | Space Complexity       | Details |
|---------------------------|------------------------|------------------------|---------|
| `Move(from, to)`          | O(\|from-to\|)         | O(1)                   |         |
| `Rotate(steps)`           | O(n)                   | O(1)                   |         |
| `Flip()`                  | O(n)                   | O(1)                   |         |
| `AddIfNotContains()`      | O(n)                   | O(1)                   |         |
| `Distinct()`              | O(n)                   | O(n)                   |         |
| `TrueForAll()`            | O(n)                   | O(1)                   |         |
| `SequenceEqual()`         | O(n)                   | O(1)                   |         |

### Factory Methods & Extensions

| Operation                 | Time Complexity        | Space Complexity       | Details |
|---------------------------|------------------------|------------------------|---------|
| `CreateRandom()`          | O(count)               | O(count)               |         |
| `CreateRandomStrings()`   | O(count*L)             | O(count*L)             |         |
| `ToLower()/ToUpper()`     | O(n*L)                 | O(1)                   |         |
| `ChangeCaseRange()`       | O(k*L)                 | O(1)                   |         |

## 📝 Complexity Notes

1. **Amortized Costs**:
   - Add operations have O(1) *amortized* time due to doubling strategy
   - Worst-case single Add operation is O(n) when resizing
   - Average cost over n insertions is O(1) per operation

2. **Special Cases**:
   - `Clear()`: O(1) for value types, O(n) for reference types (*)
   - `Sort()`: Uses O(1) space for iterative sorts, O(log n) for recursive sorts (**)
   - `BinarySearch()`: Requires sorted array (***)
   - `FindAll()`: Returns new collection (O(m) space for m matches) (****)

3. **Key Optimizations**:
   - Move operation shifts only affected elements
   - RemoveAll uses single-pass compaction
   - Span-based operations minimize overhead
   - Mathematical operations use direct iteration

4. **Memory Management**:
   - Growth factor: 2x (balances memory/speed)
   - Shrink threshold: 25% (prevents thrashing)
   - Compact() reduces capacity to exact count
