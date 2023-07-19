// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Engine.Validation;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal static class DesignTimeReferenceValidatorRunner
{
    private static readonly WeakCache<Compilation, CompilationModel> _compilationModelCache = new();

    public static ImmutableUserDiagnosticList Validate(
        ProjectServiceProvider serviceProvider,
        SemanticModel model,
        CompilationResult compilationResult,
        CancellationToken cancellationToken = default )
    {
        if ( !compilationResult.AspectPipelineResult.ReferenceValidators.IsEmpty )
        {
            var compilationModel = _compilationModelCache.GetOrAdd(
                model.Compilation,
                c => CompilationModel.CreateInitialInstance(
                    compilationResult.AspectPipelineConfiguration.ProjectModel,
                    c,
                    aspectRepository: new PipelineResultBasedAspectRepository( compilationResult.AspectPipelineResult ) ) );

            var validatorCache = new ConcurrentDictionary<ISymbol, ImmutableArray<ReferenceValidatorInstance>>();
            var userDiagnosticSink = new UserDiagnosticSink( compilationResult.AspectPipelineConfiguration.ClosureDiagnosticManifest );

            using var visitor = new ReferenceValidationVisitor(
                serviceProvider,
                userDiagnosticSink,
                s => validatorCache.GetOrAdd(
                    s,
                    symbol => compilationResult.AspectPipelineResult.ReferenceValidators.GetValidatorsForSymbol( symbol )
                        .SelectAsImmutableArray( x => x.ToReferenceValidationInstance( compilationModel ) ) ),
                compilationModel,
                cancellationToken );

            visitor.Visit( model );

            return userDiagnosticSink.ToImmutable();
        }
        else
        {
            return ImmutableUserDiagnosticList.Empty;
        }
    }
}