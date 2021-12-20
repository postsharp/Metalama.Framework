// @IncludeAllSeverities

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Aspects.Diagnostics.ReportFromInitialize
{
    public class ErrorAttribute : OverrideMethodAspect
    {
        private static readonly DiagnosticDefinition _error = new( "MY001", Severity.Error, "Error" );
        private static readonly DiagnosticDefinition _warning = new( "MY002", Severity.Warning, "Warning" );
        private static readonly DiagnosticDefinition _info = new( "MY003", Severity.Info,"Info" );
        private static readonly DiagnosticDefinition _hidden = new( "MY004", Severity.Hidden,"Hidden" );

        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            _error.ReportTo( builder.Diagnostics );
            _warning.ReportTo( builder.Diagnostics );
            _info.ReportTo( builder.Diagnostics );
            _hidden.ReportTo( builder.Diagnostics );
        }

        public override dynamic? OverrideMethod()
        {
            throw new NotImplementedException("This code should not be emitted.");
        }
    }

    // <target>
    internal class TargetClass
    {
        [Error]
        public static int Add(int a, int b)
        {
            if (a == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(a));
            }

            return a + b;
        }
    }
}
