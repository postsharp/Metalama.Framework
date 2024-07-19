#if TEST_OPTIONS
// @TestScenario(ApplyCodeFix)
#endif

using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;

namespace Metalama.Framework.Tests.Integration.CodeFixes.Suggest
{
    internal class Aspect : MethodAspect
    {
        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            base.BuildAspect( builder );

            builder.Diagnostics.Suggest( CodeFixFactory.AddAttribute( builder.Target, typeof(MyAttribute) ), builder.Target );
        }
    }

    internal class MyAttribute : Attribute { }

    // <target>
    internal class TargetCode
    {
        [Aspect]
        private int Method( int a )
        {
            return a;
        }
    }
}