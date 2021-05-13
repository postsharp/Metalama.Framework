using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.TestApp.Aspects;


namespace Caravela.Framework.TestApp
{
    internal class PrintDebugInfoAspect : OverrideMethodAspect
    {
        public override void Initialize( IAspectBuilder<IMethod> aspectBuilder )
        {
         //   Debugger.Launch();
            base.Initialize( aspectBuilder );

            aspectBuilder.Diagnostics.Report( Diagnostics.Severity.Warning, "MY000", "Hello, {0} v5.", aspectBuilder.TargetDeclaration );
        }
        public override dynamic OverrideMethod()
        {
            Console.WriteLine( DebugInfo.GetInfo() );
            return meta.Proceed();
        }
    }
}
