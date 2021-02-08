using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.TestApp.Aspects;

namespace Caravela.Framework.TestApp
{
    internal class PrintDebugInfoAspect : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            Console.WriteLine( DebugInfo.GetInfo() );
            return TemplateContext.proceed();
        }
    }
}
