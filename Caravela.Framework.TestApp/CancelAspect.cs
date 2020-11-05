using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;
using System.Linq;
using System.Threading;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.TestApp
{
    class CancelAspect : Attribute, IAspect<IMethod>
    {
        public void Initialize( IAspectBuilder<IMethod> aspectBuilder ) { }

        [OverrideMethod]
        dynamic Cancel()
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
