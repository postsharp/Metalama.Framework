using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.TestApp
{
    class PrintDebugInfoAspect : Attribute, IAspect<IMethod>
    {
        public void Initialize( IAspectBuilder<IMethod> aspectBuilder ) { }

        [OverrideMethod]
        public dynamic Template()
        {
            Console.WriteLine( DebugInfo.GetInfo() );
            return TemplateContext.proceed();
        }
    }
}
