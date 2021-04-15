// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.DesignTime
{
    /// <summary>
    /// A fake analyzer that has the desired side effect of initializing <see cref="CompilerServiceProvider"/>.
    /// </summary>
    [DiagnosticAnalyzer( LanguageNames.CSharp )]
    public class DesignTimeAnalyzer : DiagnosticAnalyzer
    {
        static DesignTimeAnalyzer()
        {
            CompilerServiceProvider.Initialize();
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray<DiagnosticDescriptor>.Empty;

        public override void Initialize( AnalysisContext context ) { }
    }
}