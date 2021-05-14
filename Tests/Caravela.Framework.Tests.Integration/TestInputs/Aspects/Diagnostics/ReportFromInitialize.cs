// @IncludeAllSeverities

using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Aspects.Diagnostics.ReportFromInitialize
{
    public class ErrorAttribute : OverrideMethodAspect
    {
        private static readonly DiagnosticDefinition _error = new( "MY001", Severity.Error, "Error" );
        private static readonly DiagnosticDefinition _warning = new( "MY002", Severity.Warning, "Warning" );
        private static readonly DiagnosticDefinition _info = new( "MY003", Severity.Info,"Info" );
        private static readonly DiagnosticDefinition _hidden = new( "MY004", Severity.Hidden,"Hidden" );

        public override void Initialize(IAspectBuilder<IMethod> aspectBuilder)
        {
            aspectBuilder.Diagnostics.Report( _error );
            aspectBuilder.Diagnostics.Report( _warning );
            aspectBuilder.Diagnostics.Report( _info );
            aspectBuilder.Diagnostics.Report(  _hidden );
        }

        public override dynamic? OverrideMethod()
        {
            throw new NotImplementedException("This code should not be emitted.");
        }
    }

    [TestOutput]
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
