# Large Object Heap Fix For .NET StringBuilder
## The suggested fix prevents .NET StringBuilder from allocating on Large Object Heap.

In the current implementation of StringBuilder, *AppendHelper* calls *ExpandByABlock(int minBlockCharCount)* which will obviously allocate on Large Object Heap when *minBlockCharCount* is large:  


```
...
m_ChunkChars = new char[newBlockLength];
...
```



Hence, defeating the purpose of the whole design of StringBuilder as described here:

(https://referencesource.microsoft.com/#mscorlib/system/text/stringbuilder.cs)


```
// We want to keep chunk arrays out of large object heap (< 85K bytes ~ 40K chars) to be sure.
// Making the maximum chunk size big means less allocation code called, but also more waste
// in unused characters and slower inserts / replaces (since you do need to slide characters over
// within a buffer).  

internal const int MaxChunkSize = 8000;
```
        
        
Instead, we are calling *AppendHelper* on smaller chunks which will result in allocations on Small Object Heap ONLY!

*Note that in order to reduce performance impact we could use bigger chunks for allocation, say three times MaxChunkSize.* 
