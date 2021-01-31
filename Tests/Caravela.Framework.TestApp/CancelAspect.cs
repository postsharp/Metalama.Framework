using Caravela.Framework.Aspects;
using System;
using System.Linq;
using System.Threading;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.TestApp
{
    class CancelAspect : OverrideMethodAspect
    {
        static bool TypeIsCancellationToken( IAdviceParameter p ) => p.Type.Is( typeof( CancellationToken ) );

        public override dynamic OverrideMethod()
        {
            Console.WriteLine( "Hello, world." );
            // error CR0101: This C# language feature is not supported by the template compiler - ParenthesizedLambdaExpression.
            // var parameter = target.Parameters.LastOrDefault( p => p.Type.Is( typeof( CancellationToken ) ) );

            var parameter = target.Parameters.LastOrDefault( TypeIsCancellationToken );

            if ( parameter != null )
            {
                parameter.Value.ThrowIfCancellationRequested();
            }

            return proceed();
        }
    }
}
