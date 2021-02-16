using System;
using System.Threading;
using Caravela.Framework.TestApp.Aspects;

namespace Caravela.Framework.TestApp
{
    [IntroduceSomeMethodAspect]
    internal class Program
    {
        private static void Main()
        {
            typeof( Program ).GetMethod( "SomeIntroducedMethod" )?.Invoke( null, null );

            PrintDebugInfo();

            PrintArray();

            ThisAccess.Run();

            new ClassWithMethods();

            Cancel();
        }

        [PrintDebugInfoAspect]
        private static void PrintDebugInfo() { }

        private static void PrintArray()
        {
            var a = new[] { 1, 2, 3, 4 };

            for ( var i = 0; i < 10; i++ )
            {
                PrintArrayAtIndex( a, i );
            }
        }

        [SwallowExceptionsAspect]
        private static void PrintArrayAtIndex( int[] a, int i )
        {
            Console.WriteLine( a[i] );
            Thread.Sleep( 100 );
        }

        private static void Cancel()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            Cancellable0();
            Cancellable1( cts.Token );
        }

        [CancelAspect]
        private static void Cancellable0() { }

        [CancelAspect]
        private static void Cancellable1( CancellationToken ct ) { }

        [CancelAspect]
        private static void Cancellable2( CancellationToken ct1, CancellationToken ct2 ) { }
    }

    [CountMethodsAspect]
    internal class ClassWithMethods
    {
        public ClassWithMethods()
        {
            this.M1();
            M2();
        }

        private void M1() { }

        private static void M2() { }
    }
}
