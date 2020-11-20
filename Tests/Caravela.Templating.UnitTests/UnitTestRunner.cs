using Caravela.TestFramework.Templating;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Caravela.Templating.UnitTests
{
    public class UnitTestRunner : TestRunner
    {
        private readonly ITestOutputHelper _logger;

        public UnitTestRunner( ITestOutputHelper logger )
        {
            _logger = logger;
        }

        protected override void ReportDiagnostics( TestResult result, IReadOnlyList<Diagnostic> diagnostics )
        {
            base.ReportDiagnostics( result, diagnostics );

            var diagnosticsToLog = diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ).ToList();

            if ( diagnosticsToLog.Count > 0 )
            {
                foreach ( var d in diagnosticsToLog )
                {
                    _logger.WriteLine( d.Location + ":" + d.Id + " " + d.GetMessage() );
                }
            }
        }
    }
}
