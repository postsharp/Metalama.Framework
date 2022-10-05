// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

#pragma warning disable RS1026 // Enable concurrent execution
#pragma warning disable RS1025 // Configure generated code analysis

namespace Metalama.Framework.CompilerExtensions
{
    // ReSharper disable UnusedType.Global

    [DiagnosticAnalyzer( LanguageNames.CSharp )]
    public class MetalamaDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private readonly DiagnosticAnalyzer? _impl;

        public MetalamaDiagnosticAnalyzer()
        {
            switch ( ProcessKindHelper.CurrentProcessKind )
            {
                case ProcessKind.Compiler:
                    break;

                case ProcessKind.DevEnv:
                    /*
                    this._impl = (DiagnosticAnalyzer) ResourceExtractor.CreateInstance(
                      "Metalama.Framework.DesignTime",
                      "Metalama.Framework.DesignTime.DefinitionOnlyDiagnosticAnalyzer" );
                    */
                    break;

                case ProcessKind.RoslynCodeAnalysisService:
                    this._impl = (DiagnosticAnalyzer) ResourceExtractor.CreateInstance(
                        "Metalama.Framework.DesignTime.VisualStudio",
                        "Metalama.Framework.DesignTime.VisualStudio.VsAnalysisProcessDiagnosticAnalyzer" );

                    break;

                default:
                    this._impl = (DiagnosticAnalyzer) ResourceExtractor.CreateInstance(
                        "Metalama.Framework.DesignTime",
                        "Metalama.Framework.DesignTime.TheDiagnosticAnalyzer" );

                    break;
            }
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => this._impl?.SupportedDiagnostics ?? ImmutableArray<DiagnosticDescriptor>.Empty;

        public override void Initialize( AnalysisContext context ) => this._impl?.Initialize( context );
    }
}