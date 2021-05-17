// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl
{
    /// <summary>
    /// Our implementation of <see cref="DiagnosticSuppressor"/>.
    /// </summary>
    [DiagnosticAnalyzer( LanguageNames.CSharp )]
    public class FacadeSuppressor : DiagnosticSuppressor
    {
        private readonly DiagnosticSuppressor _impl;

        public FacadeSuppressor()
        {
            this._impl = (DiagnosticSuppressor) ModuleInitializer.GetImplementationType( "Caravela.Framework.Impl.DesignTime.DesignTimeDiagnosticSuppressor" );
        }

        public override void ReportSuppressions( SuppressionAnalysisContext context ) => this._impl.ReportSuppressions( context );

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => this._impl.SupportedSuppressions;
    }
}