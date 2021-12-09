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
        private static readonly DiagnosticDefinition<None> _error = new( "MY001", Severity.Error, "Error" );
        private static readonly DiagnosticDefinition<None> _warning = new( "MY002", Severity.Warning, "Warning" );
        private static readonly DiagnosticDefinition<None> _info = new( "MY003", Severity.Info,"Info" );
        private static readonly DiagnosticDefinition<None> _hidden = new( "MY004", Severity.Hidden,"Hidden" );

        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            builder.Diagnostics.Report( builder.Target, _error, default );
            builder.Diagnostics.Report( builder.Target, _warning, default );
            builder.Diagnostics.Report( builder.Target, _info, default );
            builder.Diagnostics.Report( builder.Target, _hidden, default );
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
