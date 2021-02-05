using Caravela.Framework.Aspects;
using Caravela.Framework.TestApp.Aspects;
using System;

namespace Caravela.Framework.TestApp
{
    class PrintDebugInfoAspect : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            Console.WriteLine( DebugInfo.GetInfo() );
            return TemplateContext.proceed();
        }
    }
}
