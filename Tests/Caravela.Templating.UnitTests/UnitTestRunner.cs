using Caravela.Framework.Impl;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Caravela.Templating.UnitTests
{
    public class UnitTestRunner : TestRunner
    {
        private readonly ITestOutputHelper _logger;
        private readonly UsedSyntaxKindsCollector _usedSyntaxKindsCollector = new UsedSyntaxKindsCollector();
        private readonly HashSet<SyntaxKind> _unsupportedSyntaxKinds = new HashSet<SyntaxKind>();

        public UnitTestRunner( ITestOutputHelper logger )
        {
            _logger = logger;
        }

        public async Task<TestResult> Run( TestInput testInput, [CallerMemberName] string callerName = null )
        {
            var result = await base.Run( testInput );

            if ( !string.IsNullOrEmpty( callerName ) )
            {
                string syntaxKindsText = string.Join(
                    Environment.NewLine,
                    _usedSyntaxKindsCollector.CollectedSyntaxKinds
                        .Select( s => _unsupportedSyntaxKinds.Contains( s ) ? $"{s}*" : $"{s}" )
                        .OrderBy( s => s ) );

                string dirPath = Path.GetFullPath(@"..\..\..\tests\SyntaxCover");
                string filePath = Path.Combine( dirPath, callerName + ".txt" );
                Directory.CreateDirectory( dirPath );
                File.WriteAllText( filePath, syntaxKindsText );
            }

            return result;
        }

        protected override IEnumerable<CSharpSyntaxVisitor> GetTemplateAnalyzers()
        {
            yield return _usedSyntaxKindsCollector;
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

                    if ( d.Id.Equals( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id, StringComparison.Ordinal ) )
                    {
                        this.RecordUnsupportedSyntaxKind( d );
                    }
                }
            }
        }

        private void RecordUnsupportedSyntaxKind( Diagnostic diagnostic )
        {
            string syntaxKindString;
            if ( diagnostic.Properties.TryGetValue( TemplatingDiagnosticProperties.SyntaxKind, out syntaxKindString ) )
            {
                object? syntaxKind;
                if ( Enum.TryParse( typeof( SyntaxKind ), syntaxKindString, out syntaxKind ) )
                {
                    _unsupportedSyntaxKinds.Add( (SyntaxKind) syntaxKind );
                }
            }
        }
    }
}
