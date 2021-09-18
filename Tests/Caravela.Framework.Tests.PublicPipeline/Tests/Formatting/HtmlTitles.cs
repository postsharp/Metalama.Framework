// @AddHtmlTitles

using Caravela.TestFramework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.IfStatements.HtmlTitles
{
    class RunTimeClass
    {
        public void RunTimeMethod()
        {
        }
    }

    [CompileTimeOnly]
    class CompileTimeClass
    {
        public void CompileTimeMethod()
        {
        }
    }
    

    // Base list should be documented.
    [CompileTime]
    class Aspect : OverrideMethodAspect, IAspect<INamedType>
    {
        
        // Override should be documented.
        public override dynamic? OverrideMethod()
        {
            throw new System.NotImplementedException();
        }

        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            
        }

        // Explicit interface implementation should be documented.
        void IAspect<INamedType>.BuildAspect( IAspectBuilder<INamedType> builder ) { }

        [Template]
        dynamic? Template()
        {
            var m = meta.Target.Method;
            var p = meta.Target.Method.Parameters[0].Value;
            
            return meta.Proceed();
        }
    }
}
