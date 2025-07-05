Complexity Analysis of DynamicArray Operations
Below is the complete complexity analysis for all operations in the DynamicArray<T> class. The analysis uses Big O notation with:

n: Current element count (Count)
m: Elements in input collection
k: Elements affected by operation
L: Average string length (for string operations)

Operation	Time Complexity	Space Complexity	Details

Constructors			
DynamicArray(capacity)	    O(1)	O(capacity)	
DynamicArray(collection)	  O(m)	O(m)	

Add Operations			
Add(value)	                O(1) amortized	O(1) amortized	
AddRange(IEnumerable)	      O(m)	O(n+m) amortized	
AddRange(ReadOnlySpan)  	  O(m)	O(n+m) amortized	

Insert Operations			
Insert(index, value)	      O(n)	O(1) amortized	
InsertRange(index, items)	  O(n + m)	O(n+m) amortized	

Remove Operations			
RemoveAt(index)            	O(n)	O(1)	
Remove(item)	              O(n)	O(1)	
RemoveLast()	              O(1)	O(1)	
RemoveRange(start, end)    	O(n)	O(1)	
RemoveAll(match)	          O(n)	O(1)	
Clear()	                    O(1) or O(n)	O(1)	*

Mathematical			
Sum(), Average()	          O(n)	O(1)	
Min(), Max()	              O(n)	O(1)	
Median()	                  O(n log n)	O(n)	

Sorting & Reversing			
Sort()	                    O(n log n)	O(1) or O(log n)	**
Reverse()	                  O(n)	O(1)	

Search Operations			
IndexOf()                   O(n)	O(1)	
LastIndexOf()	              O(n)	O(1)	
Contains()	                O(n)	O(1)	
BinarySearch()	            O(log n)	O(1)	***
Find(), FindAll()          	O(n)	O(1) or O(m)	****
FindIndex()	                O(n)	O(1)	

Utility Operations			
ToArray()	                  O(n)	O(n)	
Swap(i, j)	                O(1)	O(1)	
Shuffle()                	  O(n)	O(1)	
TrimExcess()            	  O(n)	O(n)	
Clone()	                    O(n)	O(n)	
ConvertAll()	              O(n)	O(n)	
Move(from, to)	            O(|from-to|)	O(1)	
Replace()	                  O(n)	O(1)	

Capacity Management			
EnsureCapacity()	          O(n)	O(newCapacity)	
Compact()	                  O(n)	O(n)	
Resize()	                  O(n)	O(newSize)	

Advanced Operations			
Rotate(steps)	              O(n)	O(1)	
Flip()	                    O(n)	O(1)

Set Operations			
AddIfNotContains()	        O(n)	O(1)	
Distinct()	                O(n)	O(n)	

Verification			
TrueForAll()	              O(n)	O(1)	
SequenceEqual()	            O(n)	O(1)	

Factory Methods			
CreateRandom()	            O(count)	O(count)	
CreateRandomStrings()	      O(count*L)	O(count*L)	

String Extensions			
ToLower(), ToUpper()	      O(n*L)	O(1)	
ChangeCaseRange()	          O(k*L)	O(1)	

Complexity Notes
Amortized Costs:

Add operations have O(1) amortized time due to doubling strategy
Worst-case single Add operation is O(n) when resizing
Average cost over n insertions is O(1) per operation

Special Cases:
Clear(): O(1) for value types, O(n) for reference types
Sort(): Uses O(1) space for iterative sorts, O(log n) for recursive sorts
BinarySearch(): Requires sorted array
FindAll(): Returns new collection (O(m) space for m matches)

Key Optimizations:
Move operation shifts only affected elements
RemoveAll uses single-pass compaction
Span-based operations minimize overhead
Mathematical operations use direct iteration

Efficiency Highlights:
Backed by array buffer for memory locality
Growth factor (2x) balances memory/speed
Shrink threshold (25%) prevents thrashing
Specialized implementations for value types

This complexity analysis shows that DynamicArray provides performance characteristics comparable to List<T> while offering additional functionality and 
optimizations for specific use cases.
