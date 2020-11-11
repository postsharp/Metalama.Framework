using System;
using System.Threading;

namespace Caravela.Framework.TestApp
{
    class Program
    {
        static void Main()
        {
            PrintDebugInfo();

            PrintArray();

            ThisAccess.Run();

            new ClassWithMethods();

            Cancel();
        }

        [PrintDebugInfoAspect]
        static void PrintDebugInfo() { }

        static void PrintArray()
        {
            var a = new[] { 1, 2, 3, 4 };

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

        static void Cancel()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            Cancellable0();
            Cancellable1( cts.Token );
        }

        [CancelAspect] static void Cancellable0() { }
        [CancelAspect] static void Cancellable1( CancellationToken ct ) { }
        [CancelAspect] static void Cancellable2( CancellationToken ct1, CancellationToken ct2 ) { }
    }

    [CountMethodsAspect]
    class ClassWithMethods
    {
        public ClassWithMethods()
        {
            this.M1();
            M2();
        }

        void M1() { }

        static void M2() { }
    }
}
