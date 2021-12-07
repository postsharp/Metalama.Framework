using System;

namespace Caravela.Framework.Tests.Integration.Tests.Formatting.IfDirective
{
    public class SomeClass
    {
        void Main()
        {
#if CARAVELA
            Console.WriteLine("This should be included.");
#else
            Console.WriteLine( "This should NOT be included." );
#endif

#if CARAVELA
            Console.WriteLine("This should be included.");
#elif NOTHING
            Console.WriteLine( "This should NOT be included." );
#endif
            
        }
        
    }
}