using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using System.Linq;
using System.Threading;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.TestApp
{
    class CancelAspect : OverrideMethodAspect
    {
        [OverrideMethodTemplate]
        public dynamic Template()
        {
            var parameter = target.Parameters.LastOrDefault( p => p.Type.Is( typeof( CancellationToken ) ) );

            if ( parameter != null )
            {
                parameter.Value.ThrowIfCancellationRequested();
            }

            return proceed();
        }
    }
}
