using Caravela.TestFramework;
using Caravela.TestFramework.Templating;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Caravela.Framework.Aspects.UnitTests
{
    public class AspectUnitTestRunner : AspectTestRunner
    {
        private readonly ITestOutputHelper _logger;
        
        public AspectUnitTestRunner( ITestOutputHelper logger )
        {
            this._logger = logger;
        }

        protected override void ReportDiagnostics( TestResult result, IReadOnlyList<Diagnostic> diagnostics )
        {
            base.ReportDiagnostics( result, diagnostics );

            var diagnosticsToLog = diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ).ToList();

            if ( diagnosticsToLog.Count > 0 )
            {
                foreach ( var d in diagnosticsToLog )
                {
                    this._logger.WriteLine( d.Location + ":" + d.Id + " " + d.GetMessage() );
                }
            }
        }
    }
}
