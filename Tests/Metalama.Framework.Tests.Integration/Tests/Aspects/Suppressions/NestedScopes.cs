using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Tests.Integration.Aspects.Suppressions.NestedScopes
{
    public class SuppressWarningAttribute : MethodAspect
    {
        private static readonly SuppressionDefinition _suppression = new( "CS0219" );

        public SuppressWarningAttribute() { }

        [Template]
        public dynamic? Override()
        {
#if !TESTRUNNER // Disable the warning in the main build, not during tests. (1)
#pragma warning disable CS0219
#endif
            var a = 0;

            return meta.Proceed();
        }

        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.Advices.Override( builder.Target, nameof(Override) );
            builder.Diagnostics.Suppress( _suppression, builder.Target );
        }
    }

    // <target>
    internal class TargetClass
    {
        [SuppressWarning]
        private void M2( string m )
        {
#pragma warning disable CS0219
            var x = 0;
#pragma warning restore CS0219

#if !TESTRUNNER // Disable the warning in the main build, not during tests. (1)
#pragma warning disable CS0219
#endif

            var y = 0;
        }

        private void M1( string m )
        {
#pragma warning disable CS0219
            var x = 0;
#pragma warning restore CS0219

#if !TESTRUNNER // Disable the warning in the main build, not during tests. (2)
#pragma warning disable CS0219, 219
#endif

            // CS0219 expected 
            var y = 0;
        }
    }
}