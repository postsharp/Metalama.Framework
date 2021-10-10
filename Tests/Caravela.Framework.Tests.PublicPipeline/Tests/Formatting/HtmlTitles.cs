using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.IfStatements.HtmlTitles
{
    internal class RunTimeClass
    {
        public void RunTimeMethod() { }
    }

    [CompileTimeOnly]
    internal class CompileTimeClass
    {
        public void CompileTimeMethod() { }
    }

    // Base list should be documented.
    [CompileTime]
    internal class Aspect : OverrideMethodAspect, IAspect<INamedType>
    {
        // Override should be documented.
        public override dynamic? OverrideMethod()
        {
            throw new NotImplementedException();
        }

        public override void BuildAspect( IAspectBuilder<IMethod> builder ) { }

        // Explicit interface implementation should be documented.
        void IAspect<INamedType>.BuildAspect( IAspectBuilder<INamedType> builder ) { }

        [Template]
        private dynamic? Template()
        {
            var m = meta.Target.Method;
            var p = meta.Target.Method.Parameters[0].Value;

            return meta.Proceed();
        }

        void IEligible<INamedType>.BuildEligibility( IEligibilityBuilder<INamedType> builder ) { }
    }
}