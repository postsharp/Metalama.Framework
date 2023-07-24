// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Options;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime;

internal sealed class SuppressionAnalysisContextAdapter : ISuppressionAnalysisContext
{
    private readonly SuppressionAnalysisContext _context;

    public SuppressionAnalysisContextAdapter( SuppressionAnalysisContext context )
    {
        this._context = context;
    }

    public Compilation Compilation => this._context.Compilation;

    public IProjectOptions ProjectOptions => MSBuildProjectOptionsFactory.Default.GetProjectOptions( this._context.Options.AnalyzerConfigOptionsProvider );

    public CancellationToken CancellationToken => this._context.CancellationToken;

    public ImmutableArray<Diagnostic> ReportedDiagnostics => this._context.ReportedDiagnostics;

    // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
    public void ReportSuppression( Suppression suppression ) => this._context.ReportSuppression( suppression );
}