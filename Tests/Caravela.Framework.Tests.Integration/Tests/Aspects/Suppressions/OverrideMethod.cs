#if !TESTRUNNER

// Disable the warning in the main build, not during tests.
#pragma warning disable CS0219
#endif

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Tests.Integration.Aspects.Suppressions.IntroduceMethod
{
    public class SuppressWarningAttribute : MethodAspect
    {
        private static readonly SuppressionDefinition _suppression = new( "CS0219" );

        public SuppressWarningAttribute() { }

        [Template]
        public dynamic? Override()
        {
            var a = 0;

            return meta.Proceed();
        }

        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.Advices.OverrideMethod( builder.Target, nameof(Override) );
            builder.Diagnostics.Suppress( null, _suppression );
        }
    }

    // <target>
    internal class TargetClass
    {
        [SuppressWarning]
        private void M2( string m )
        {
            var x = 0;
        }

        // CS0219 expected 
        private void M1( string m )
        {
            var x = 0;
        }
    }
}