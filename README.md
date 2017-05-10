# Large Object Heap Fix For .NET StringBuilder
## The suggested fix prevents .NET StringBuilder from allocating on the Large Object Heap.

In the current implementation of .NET 4.5 or higher each time we call *StringBuilder's* *Append(string value)*, it internally decides whether to use an existing buffer *m_ChunkChars* or expand itself by creating another instance of *StringBuilder.*

If so *AppendHelper* method calls *ExpandByABlock(int minBlockCharCount)* which will obviously allocate on the Large Object Heap if *minBlockCharCount* is large:  


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
        
        
To fix this issue, we simply call *AppendHelper* on smaller chunks which will result in allocations on the Small Object Heap ONLY:

```
private void AppendHelper(string value) {
    unsafe {
        fixed (char* valueChars = value)
        {
            if (value.Length <= MaxChunkSize)
            {
                // regular case
                Append(valueChars, value.Length);
            }
            else
            {
                //
                // possibly large allocation, so do it in smaller chunks
                //
                int numOfChunks = value.Length / MaxChunkSize;
                int remainder = value.Length % MaxChunkSize;
                for (int i = 0; i < numOfChunks; ++i)
                {
                    Append(valueChars + (i * MaxChunkSize), MaxChunkSize);
                }
                if (remainder > 0)
                {
                    Append(valueChars + value.Length - remainder, remainder);
                }
            }
        }
    }
}
```

*Note that in order to reduce performance impact we could use bigger chunks for allocation, say five times MaxChunkSize.* 
