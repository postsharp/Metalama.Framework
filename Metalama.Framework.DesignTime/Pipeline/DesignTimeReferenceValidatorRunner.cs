// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Engine.Utilities.Threading;
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
            var validatorProvider = new ValidatorProvider( aspectPipelineResultAndState.Result.ReferenceValidators, compilationModel );
            var taskRunner = serviceProvider.Global.GetRequiredService<ITaskRunner>();
            var referenceRunner = new ReferenceValidatorRunner( serviceProvider );

            taskRunner.RunSynchronously(
                () => referenceRunner.RunReferenceValidatorsAsync( compilationModel, model, userDiagnosticSink, validatorProvider, cancellationToken ),
                cancellationToken );

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

        public ReferenceIndexerOptions Options => this._validators.Options;

        public ImmutableArray<ReferenceValidatorInstance> GetValidators( ISymbol symbol )
            => this._validatorCache.GetOrAdd(
                symbol,
                static ( s, o ) => o._validators.GetValidatorsForSymbol( s )
                    .SelectAsImmutableArray( x => x.ToReferenceValidationInstance( o._compilationModel ) ),
                this );
    }
}