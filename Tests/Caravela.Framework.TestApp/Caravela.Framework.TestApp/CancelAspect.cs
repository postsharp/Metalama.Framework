using System.Linq;
using System.Threading;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.TestApp
{
    internal class CancelAspect : OverrideMethodAspect
    {
        private static bool TypeIsCancellationToken( IAdviceParameter p ) => p.ParameterType.Is( typeof( CancellationToken ) );

        public override dynamic OverrideMethod()
        {
            System.Console.WriteLine( "Hello, world." );

            
             var parameter = meta.Parameters.LastOrDefault( p => p.ParameterType.Is( typeof( CancellationToken ) ) );

            if ( parameter != null )
            {
                parameter.Value.ThrowIfCancellationRequested();
            }

            return meta.Proceed();
        }
    }
}
