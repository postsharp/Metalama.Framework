using System;
using System.Threading;
using System.Threading.Tasks;
using Caravela.Framework.TestApp.Aspects;

namespace Caravela.Framework.TestApp
{
    [IntroduceSomeMethodAspect]
    internal partial class Program
    {
        [SuppressWarning("CS1998", "IDE0051")]
        private static async Task MethodAsync()
        {
            
        }

        private static void Main()
        {
            //Console.WriteLine( $"x={x}" );

            SomeIntroducedMethod();
            (new Program()).SomeOtherIntroducedMethod();

           // IMethod m = null;
            // m.Base.Invoke( null );

            // TemplateContext.compileTime( 0 );

            MethodWithTwoAspects();

            PrintDebugInfo();

            PrintArray();

            ThisAccess.Run();
            
            Cancel();
        }

        [SwallowExceptionsAspect]
        [PrintDebugInfoAspect]
        public static void MethodWithTwoAspects()
        {
            Console.WriteLine( "This is method with two aspects" );
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

    
}
