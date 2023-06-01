// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Validation;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal sealed class DesignTimeValidatorRunner
{
    private readonly ProjectServiceProvider _serviceProvider;
    private readonly CompilationPipelineResult _compilationResult;
    private readonly CompilationModel _compilation;
    private readonly ConcurrentDictionary<ISymbol, ImmutableArray<ReferenceValidatorInstance>> _validatorCache = new();

    public DesignTimeValidatorRunner(
        ProjectServiceProvider serviceProvider,
        CompilationPipelineResult compilationResult,
        ProjectModel project,
        PartialCompilation compilation )
    {
        this._serviceProvider = serviceProvider;
        this._compilationResult = compilationResult;
        this._compilation = CompilationModel.CreateInitialInstance( project, compilation, new PipelineResultBasedAspectRepository( compilationResult ) );
    }

    public void Validate( SemanticModel model, UserDiagnosticSink diagnosticSink, CancellationToken cancellationToken )
    {
        if ( !this._compilationResult.Validators.IsEmpty )
        {
            using var visitor = new ReferenceValidationVisitor(
                this._serviceProvider,
                diagnosticSink,
                s => this._validatorCache.GetOrAdd(
                    s,
                    symbol => this._compilationResult.Validators.GetValidatorsForSymbol( symbol )
                        .SelectAsImmutableArray( x => x.ToReferenceValidationInstance( this._compilation ) ) ),
                this._compilation,
                cancellationToken );

            visitor.Visit( model );
        }
    }
}