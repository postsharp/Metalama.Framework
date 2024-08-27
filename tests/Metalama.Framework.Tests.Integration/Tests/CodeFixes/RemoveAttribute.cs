#if TEST_OPTIONS
// @TestScenario(CodeFix)
#endif

using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Tests.Integration.CodeFixes.RemoveAttribute
{
    internal class Aspect : MethodAspect
    {
        private static DiagnosticDefinition _diag = new( "MY001", Severity.Warning, "Add some attribute" );

        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            base.BuildAspect( builder );

            builder.Diagnostics.Report( _diag.WithCodeFixes( CodeFixFactory.RemoveAttributes( builder.Target.DeclaringType, typeof(MyAttribute) ) ) );
        }
    }

    internal class MyAttribute : Attribute { }

    internal class YourAttribute : Attribute { }

// <target>
    internal class T
    {
        internal partial class TargetCode
        {
            [Aspect]
            [My]
            private int Method1( int a )
            {
                return a;
            }

            [My]
            private int Method2( int a )
            {
                return a;
            }
        }

        internal partial class TargetCode
        {
            [My]
            [Your]
            private int Method3( int a )
            {
                return a;
            }
        }
    }
}