#if TEST_OPTIONS
// @ApplyCodeFix
#endif

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;

namespace Metalama.Framework.Tests.Integration.CodeFixes.ChangeVisibility_Partial
{
    internal class Aspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            base.BuildAspect( builder );

            builder.Diagnostics.Suggest( CodeFixFactory.ChangeAccessibility( builder.Target, Accessibility.Public ) );
        }
    }

    internal class MyAttribute : Attribute { }

    // <target>
    internal class T
    {
        [Aspect]
        internal partial class TargetCode
        {
            private int Method( int a )
            {
                return a;
            }
        }

        internal partial class TargetCode { }
    }
}