// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Options;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Metalama.Framework.DesignTime;

internal sealed class SemanticModelAnalysisContextAdapter : ISemanticModelAnalysisContext
{
    private readonly SemanticModelAnalysisContext _context;

    public SemanticModelAnalysisContextAdapter( SemanticModelAnalysisContext context )
    {
        this._context = context;
    }

    public SemanticModel SemanticModel => this._context.SemanticModel;

    public CancellationToken CancellationToken => this._context.CancellationToken;

    public IProjectOptions ProjectOptions => MSBuildProjectOptionsFactory.Default.GetProjectOptions( this._context.Options.AnalyzerConfigOptionsProvider );

    // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
    public void ReportDiagnostic( Diagnostic diagnostic ) => this._context.ReportDiagnostic( diagnostic );
}