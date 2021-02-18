using System.Linq;
using System.Threading;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.TestApp
{
    internal class CancelAspect : OverrideMethodAspect
    {
        private static bool TypeIsCancellationToken( IAdviceParameter p ) => p.ParameterType.Is( typeof( CancellationToken ) );

        public override dynamic OverrideMethod()
        {
            System.Console.WriteLine( "Hello, world." );

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
