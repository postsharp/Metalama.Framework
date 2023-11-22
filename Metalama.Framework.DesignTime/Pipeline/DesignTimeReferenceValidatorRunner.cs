// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Engine.Utilities.Roslyn;
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
        ISemanticModel model,
        AspectPipelineResultAndState aspectPipelineResultAndState,
        CancellationToken cancellationToken = default )
    {
        if ( !aspectPipelineResultAndState.Result.ReferenceValidators.IsEmpty )
        {
            var compilationModel = _compilationModelCache.GetOrAdd(
                model.Compilation,
                c => CompilationModel.CreateInitialInstance(
                    aspectPipelineResultAndState.Configuration.ProjectModel,
                    c,
                    aspectRepository: new PipelineResultBasedAspectRepository( aspectPipelineResultAndState.Result ) ) );

            var userDiagnosticSink = new UserDiagnosticSink( aspectPipelineResultAndState.Configuration.ClosureDiagnosticManifest );

            using var visitor = new ReferenceValidationVisitor(
                serviceProvider,
                userDiagnosticSink,
                new ValidatorProvider( aspectPipelineResultAndState.Result.ReferenceValidators, compilationModel ),
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

    private sealed class ValidatorProvider : IReferenceValidatorProvider
    {
        private readonly DesignTimeReferenceValidatorCollection _validators;
        private readonly ConcurrentDictionary<ISymbol, ImmutableArray<ReferenceValidatorInstance>> _validatorCache = new();
        private readonly CompilationModel _compilationModel;

        public ValidatorProvider( DesignTimeReferenceValidatorCollection validators, CompilationModel compilationModel )
        {
            this._validators = validators;
            this._compilationModel = compilationModel;
        }

        public ReferenceValidatorCollectionProperties Properties => this._validators.Properties;

        public ImmutableArray<ReferenceValidatorInstance> GetValidators( ISymbol symbol )
            => this._validatorCache.GetOrAdd(
                symbol,
                static ( s, o ) => o._validators.GetValidatorsForSymbol( s )
                    .SelectAsImmutableArray( x => x.ToReferenceValidationInstance( o._compilationModel ) ),
                this );
    }
}