// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Metalama.Framework.CompilerExtensions
{
    // ReSharper disable UnusedType.Global

    [DiagnosticAnalyzer( LanguageNames.CSharp )]
    public class MetalamaDiagnosticSuppressor : DiagnosticSuppressor
    {
        private readonly DiagnosticSuppressor? _impl;

        public MetalamaDiagnosticSuppressor()
        {
            switch ( ProcessKindHelper.CurrentProcessKind )
            {
                case ProcessKind.Compiler:
                    break;

                case ProcessKind.RoslynCodeAnalysisService:
                case ProcessKind.DevEnv:
                    this._impl = (DiagnosticSuppressor) ResourceExtractor.CreateInstance(
                        "Metalama.Framework.DesignTime.VisualStudio",
                        "Metalama.Framework.DesignTime.VisualStudio.VsDiagnosticSuppressor" );

                    break;

                default:
                    this._impl = (DiagnosticSuppressor) ResourceExtractor.CreateInstance(
                        "Metalama.Framework.DesignTime",
                        "Metalama.Framework.DesignTime.TheDiagnosticSuppressor" );

                    break;
            }
        }

        public override void ReportSuppressions( SuppressionAnalysisContext context ) => this._impl?.ReportSuppressions( context );

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions
            => this._impl?.SupportedSuppressions ?? ImmutableArray<SuppressionDescriptor>.Empty;
    }
}