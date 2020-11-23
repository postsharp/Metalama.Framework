using Caravela.TestFramework.Templating;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Caravela.AspectWorkbench
{
    public class WorkbenchTestRunner : TestRunner
    {
        private readonly TextBlock errorsTextBlock;

        public WorkbenchTestRunner( TextBlock errorsTextBlock )
        {
            this.errorsTextBlock = errorsTextBlock;
        }

        public new Project CreateProject()
        {
            return base.CreateProject();
        }

        protected override void ReportDiagnostics( TestResult result, IReadOnlyList<Diagnostic> diagnostics )
        {
            base.ReportDiagnostics( result, diagnostics );

            var diagnosticsToLog = diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ).ToList();

            if ( diagnosticsToLog.Count > 0 )
            {
                if ( this.errorsTextBlock.Text.Length > 0 )
                {
                    this.errorsTextBlock.Text += Environment.NewLine;
                }

                this.errorsTextBlock.Text += string.Join( Environment.NewLine,
                    diagnosticsToLog.Select( d => d.Location + ":" + d.Id + " " + d.GetMessage() ) );
            }
        }
    }
}
