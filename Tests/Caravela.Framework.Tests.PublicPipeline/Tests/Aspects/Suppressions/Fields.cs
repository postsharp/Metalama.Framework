#if !TESTRUNNER

// Disable the warning in the main build, not during tests.
#pragma warning disable CS0169
#endif

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Tests.Integration.Aspects.Suppressions.Fields
{
    public class SuppressWarningAttribute : FieldAspect
    {
        private static readonly SuppressionDefinition _suppression1 = new( "CS0169" );
        private static readonly SuppressionDefinition _suppression2 = new( "CS0649" );

        public SuppressWarningAttribute() { }

        public override void BuildAspect( IAspectBuilder<IField> builder )
        {
            builder.Diagnostics.Suppress( null, _suppression1 );
            builder.Diagnostics.Suppress( null, _suppression2 );
        }
    }

    // <target>
    internal class TargetClass
    {
        // CS0169 expected here.
        private int x;

        [SuppressWarning]
        private int y;

        [SuppressWarning]
        private int w, z;
    }
}