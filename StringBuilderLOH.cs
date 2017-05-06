// https://referencesource.microsoft.com/#mscorlib/system/text/stringbuilder.cs

// ... skipped ... 

#define APPEND_HELPER_FIX

namespace System.Text {
    using System;
    using System.Text;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Threading;
    using System.Globalization;
    using System.Diagnostics.Contracts;

    // This class represents a mutable string.  It is convenient for situations in
    // which it is desirable to modify a string, perhaps by removing, replacing, or 
    // inserting characters, without creating a new String subsequent to
    // each modification. 
    // 
    // The methods contained within this class do not return a new StringBuilder
    // object unless specified otherwise.  This class may be used in conjunction with the String
    // class to carry out modifications upon strings.
    // 
    // When passing null into a constructor in VJ and VC, the null
    // should be explicitly type cast.
    // For Example:
    // StringBuilder sb1 = new StringBuilder((StringBuilder)null);
    // StringBuilder sb2 = new StringBuilder((String)null);
    // Console.WriteLine(sb1);
    // Console.WriteLine(sb2);
    // 
    [System.Runtime.InteropServices.ComVisible(true)]
    [Serializable]
    public sealed class StringBuilder : ISerializable {

        // ... skipped ... 

        // A StringBuilder is internally represented as a linked list of blocks each of which holds
        // a chunk of the string.  It turns out string as a whole can also be represented as just a chunk, 
        // so that is what we do.  

        // We want to keep chunk arrays out of large object heap (< 85K bytes ~ 40K chars) to be sure.
        // Making the maximum chunk size big means less allocation code called, but also more waste
        // in unused characters and slower inserts / replaces (since you do need to slide characters over
        // within a buffer).  
        internal const int MaxChunkSize = 8000;


        // ... skipped ... 


#if APPEND_HELPER_FIX

        // We put this fixed in its own helper to avoid the cost zero initing valueChars in the
        // case we don't actually use it.  
        [System.Security.SecuritySafeCritical]  // auto-generated
        private void AppendHelper(string value) {
            unsafe {
                fixed (char* valueChars = value)
                {
                    if (value.Length <= MaxChunkSize)
                    {
                        Append(valueChars, value.Length);
                    }
                    else
                    {
                        //
                        // do it in multiple loops
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

#else

        // We put this fixed in its own helper to avoid the cost zero initing valueChars in the
        // case we don't actually use it.  
        [System.Security.SecuritySafeCritical]  // auto-generated
        private void AppendHelper(string value) {
            unsafe {
                fixed (char* valueChars = value)
                    Append(valueChars, value.Length);
            }
        }

#endif

        // ... skipped ... 

    }
}
