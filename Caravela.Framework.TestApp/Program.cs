using System;
using System.Threading;

namespace Caravela.Framework.TestApp
{
    class Program
    {
        static void Main()
        {
            var a = new[] { 1, 2, 3, 4, 5 };

            for ( int i = 0; i < 10; i++ )
            {
                PrintArrayAtIndex( a, i );
            }
        }

        [SwallowExceptionsAspect]
        static void PrintArrayAtIndex(int[] a, int i)
        {
            Console.WriteLine( a[i] );
            Thread.Sleep( 100 );
        }
    }
}
