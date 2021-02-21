using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit.Abstractions;

namespace Caravela.TestFramework.Templating
{
    public class UnitTestRunner : TemplateTestRunner
    {
        private readonly ITestOutputHelper _logger;
        private readonly UsedSyntaxKindsCollector _usedSyntaxKindsCollector = new UsedSyntaxKindsCollector();
        private readonly HashSet<SyntaxKind> _unsupportedSyntaxKinds = new HashSet<SyntaxKind>();

        public UnitTestRunner( ITestOutputHelper logger )
        {
            this._logger = logger;
        }

        public async Task<TestResult> Run( TestInput testInput )
        {
            var result = await base.Run( testInput );

            if ( !string.IsNullOrEmpty( testInput.TestName ) && this._usedSyntaxKindsCollector.CollectedSyntaxKinds.Any() )
            {
                var syntaxKindsText = string.Join(
                    Environment.NewLine,
                    this._usedSyntaxKindsCollector.CollectedSyntaxKinds
                        .Select( s => this._unsupportedSyntaxKinds.Contains( s ) ? $"{s}*" : $"{s}" )
                        .OrderBy( s => s ) );

                var dirPath = Path.GetFullPath( @"..\..\..\tests\SyntaxCover" );
                var filePath = Path.Combine( dirPath, testInput.TestName + ".txt" );
                Directory.CreateDirectory( Path.GetDirectoryName( filePath ) );
                File.WriteAllText( filePath, syntaxKindsText );
            }

            return result;
        }

        protected override IEnumerable<CSharpSyntaxVisitor> GetTestAnalyzers()
        {
            yield return this._usedSyntaxKindsCollector;
        }

        protected override void ReportDiagnostics( TestResult result, IReadOnlyList<Diagnostic> diagnostics )
        {
            base.ReportDiagnostics( result, diagnostics );

            var diagnosticsToLog = diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ).ToList();

            if ( diagnosticsToLog.Count > 0 )
            {
                foreach ( var d in diagnosticsToLog )
                {
                    this._logger.WriteLine( d.ToString() );

                    if ( d.Id.Equals( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id, StringComparison.Ordinal ) )
                    {
                        this.RecordUnsupportedSyntaxKind( d );
                    }
                }
            }
        }

        private void RecordUnsupportedSyntaxKind( Diagnostic diagnostic )
        {
            if ( diagnostic.Properties.TryGetValue( TemplatingDiagnosticProperties.SyntaxKind, out var syntaxKindString ) )
            {
                if ( Enum.TryParse( typeof( SyntaxKind ), syntaxKindString, out var syntaxKind ) )
                {
                    this._unsupportedSyntaxKinds.Add( (SyntaxKind) syntaxKind! );
                }
            }
        }
    }
}
