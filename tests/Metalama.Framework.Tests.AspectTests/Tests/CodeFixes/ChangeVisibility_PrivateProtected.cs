#if TEST_OPTIONS
// @TestScenario(CodeFix)
#endif

using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;

namespace Metalama.Framework.Tests.AspectTests.CodeFixes.ChangeVisibility_PrivateProtected
{
    internal class Aspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            base.BuildAspect( builder );

            builder.Diagnostics.Suggest( CodeFixFactory.ChangeAccessibility( builder.Target, Accessibility.PrivateProtected ) );
        }
    }

    internal class MyAttribute : Attribute { }

    // <target>
    internal class T
    {
        [Aspect]
        internal class TargetCode
        {
            private int Method( int a )
            {
                return a;
            }
        }
    }
}