using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.TestApp.Aspects;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.TestApp
{
    internal class PrintDebugInfoAspect : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            Console.WriteLine( DebugInfo.GetInfo() );
            return proceed();
        }
    }
}
