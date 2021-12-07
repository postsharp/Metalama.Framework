using System;

namespace Metalama.Framework.Tests.Integration.Tests.Formatting.IfDirective
{
    public class SomeClass
    {
        void Main()
        {
#if METALAMA
            Console.WriteLine("This should be included.");
#else
            Console.WriteLine( "This should NOT be included." );
#endif

#if METALAMA
            Console.WriteLine("This should be included.");
#elif NOTHING
            Console.WriteLine( "This should NOT be included." );
#endif
            
        }
        
    }
}