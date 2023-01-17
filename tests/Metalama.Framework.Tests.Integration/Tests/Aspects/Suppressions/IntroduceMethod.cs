#if TEST_OPTIONS
// @Skipped (#31074)
#endif

#if !TESTRUNNER

// Disable the warning in the main build, not during tests.
#pragma warning disable CS0219
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Tests.Integration.Aspects.Suppressions.OverrideMethod
{
    public class SuppressWarningAttribute : TypeAspect
    {
        private static readonly SuppressionDefinition _suppression1 = new( "CS0219" );

        [Template]
        public void Introduced()
        {
            var x = 0;
        }

        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            var introduced = builder.Advise.IntroduceMethod( builder.Target, nameof(Introduced) ).Declaration;
            builder.Diagnostics.Suppress( _suppression1, introduced );
        }
    }

    // <target>
    [SuppressWarning]
    internal class TargetClass
    {
        // CS0219 expected 
        private void M1( string m )
        {
            var x = 0;
        }
    }
}