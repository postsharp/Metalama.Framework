#if !TESTRUNNER

// Disable the warning in the main build, not during tests.
#pragma warning disable CS0219
#endif

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Tests.Integration.Aspects.Suppressions.Methods
{
    public class SuppressWarningAttribute : MethodAspect
    {
        private static readonly SuppressionDefinition _suppression1 = new( "CS0219" );

        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.Diagnostics.Suppress( null, _suppression1 );
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