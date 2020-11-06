using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using System;

namespace Caravela.Framework.TestApp
{
    class PrintDebugInfoAspect : OverrideMethodAspect
    {
        [OverrideMethodTemplate]
        public dynamic Template()
        {
            Console.WriteLine( DebugInfo.GetInfo() );
            return TemplateContext.proceed();
        }
    }
}
