// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Fabrics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Engine.Utilities.UserCode;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Validation;

internal sealed class ValidationRunner
{
    private readonly AspectPipelineConfiguration _configuration;
    private readonly ImmutableArray<IValidatorSource> _sources;
    private readonly ProjectServiceProvider _serviceProvider;
    private readonly UserCodeInvoker _userCodeInvoker;
    private readonly IConcurrentTaskRunner _concurrentTaskRunner;

    public ValidationRunner( AspectPipelineConfiguration configuration, ImmutableArray<IValidatorSource> sources )
    {
        this._configuration = configuration;
        this._sources = sources;
        this._serviceProvider = configuration.ServiceProvider;
        this._userCodeInvoker = this._serviceProvider.GetRequiredService<UserCodeInvoker>();
        this._concurrentTaskRunner = this._serviceProvider.GetRequiredService<IConcurrentTaskRunner>();
    }

    /// <summary>
    /// Runs both declaration and reference validators.
    /// </summary>
    public async Task<ValidationResult> RunAllAsync(
        CompilationModel initialCompilation,
        CompilationModel finalCompilation,
        CancellationToken cancellationToken )
    {
        var initialCompilationWithEnhancements = initialCompilation.WithAspectRepository( finalCompilation.AspectRepository, "With final aspect repository" );

        var userDiagnosticSink = new UserDiagnosticSink( this._configuration.CompileTimeProject, this._configuration.CodeFixFilter );

        var declarationValidatorsTask = this.RunDeclarationValidatorsAsync(
            initialCompilationWithEnhancements,
            finalCompilation,
            userDiagnosticSink,
            cancellationToken );

        var referenceValidatorsTask = this.RunReferenceValidatorsAsync( initialCompilationWithEnhancements, userDiagnosticSink, cancellationToken );

        await Task.WhenAll( declarationValidatorsTask, referenceValidatorsTask );

        var hasDeclarationValidator = await declarationValidatorsTask;
        var referenceValidators = await referenceValidatorsTask;

        return new ValidationResult( hasDeclarationValidator, referenceValidators, userDiagnosticSink.ToImmutable() );
    }

    private async Task<IReadOnlyCollection<ValidatorInstance>> GetValidatorsAsync(
        ValidatorKind kind,
        CompilationModel compilation,
        CompilationModelVersion version,
        UserDiagnosticSink diagnosticAdder,
        CancellationToken cancellationToken )
    {
        var collector = new OutboundActionCollector( diagnosticAdder );

        var tasks = this._sources
            .Select( s => s.CollectValidatorsAsync( kind, version, new OutboundActionCollectionContext( collector, compilation, cancellationToken ) ) );

        await Task.WhenAll( tasks );

        return collector.Validators;
    }

    public async Task RunDeclarationValidatorsAsync(
        CompilationModel compilation,
        CompilationModelVersion version,
        UserDiagnosticSink diagnosticAdder,
        CancellationToken cancellationToken )
    {
        var validators = await this.GetValidatorsAsync( ValidatorKind.Definition, compilation, version, diagnosticAdder, cancellationToken );

        var userCodeExecutionContext = new UserCodeExecutionContext( this._serviceProvider, diagnosticAdder, default, compilationModel: compilation );

        using ( UserCodeExecutionContext.WithContext( userCodeExecutionContext ) )
        {
            foreach ( var validator in validators )
            {
                userCodeExecutionContext.Description = validator.Driver.GetUserCodeMemberInfo( validator );

                ((DeclarationValidatorInstance) validator).Validate( diagnosticAdder, this._userCodeInvoker, userCodeExecutionContext );
            }
        }
    }

    public async Task<bool> RunDeclarationValidatorsAsync(
        CompilationModel initialCompilation,
        CompilationModel finalCompilation,
        UserDiagnosticSink diagnosticAdder,
        CancellationToken cancellationToken )
    {
        var initialCompilationValidationTask = this.RunDeclarationValidatorsCoreAsync(
            initialCompilation,
            CompilationModelVersion.Initial,
            diagnosticAdder,
            cancellationToken );

        var finalCompilationValidationTask = this.RunDeclarationValidatorsCoreAsync(
            finalCompilation,
            CompilationModelVersion.Final,
            diagnosticAdder,
            cancellationToken );

        await Task.WhenAll( initialCompilationValidationTask, finalCompilationValidationTask );

        var hasInitialCompilationValidator = await initialCompilationValidationTask;
        var hasFinalCompilationValidator = await finalCompilationValidationTask;

        return hasInitialCompilationValidator || hasFinalCompilationValidator;
    }

    private async Task<bool> RunDeclarationValidatorsCoreAsync(
        CompilationModel compilation,
        CompilationModelVersion version,
        UserDiagnosticSink diagnosticAdder,
        CancellationToken cancellationToken )
    {
        var validators = await this.GetValidatorsAsync( ValidatorKind.Definition, compilation, version, diagnosticAdder, cancellationToken );

        var userCodeExecutionContext = new UserCodeExecutionContext( this._serviceProvider, diagnosticAdder, default, compilationModel: compilation );

        await this._concurrentTaskRunner.RunInParallelAsync( validators, RunValidator, cancellationToken );

        return validators.Count > 0;

        void RunValidator( ValidatorInstance validator )
        {
            using ( UserCodeExecutionContext.WithContext( userCodeExecutionContext ) )
            {
                userCodeExecutionContext.Description = validator.Driver.GetUserCodeMemberInfo( validator );

                ((DeclarationValidatorInstance) validator).Validate( diagnosticAdder, this._userCodeInvoker, userCodeExecutionContext );
            }
        }
    }

    private async Task<ImmutableArray<ReferenceValidatorInstance>> RunReferenceValidatorsAsync(
        CompilationModel initialCompilation,
        UserDiagnosticSink diagnosticAdder,
        CancellationToken cancellationToken )
    {
        var validators = await this.GetReferenceValidatorsAsync( initialCompilation, diagnosticAdder, cancellationToken );

        var validatorsBySymbol = validators
            .ToMultiValueDictionary(
                v => v.ValidatedDeclaration.GetSymbol().AssertNotNull(),
                v => v );

        if ( validatorsBySymbol.IsEmpty )
        {
            return ImmutableArray<ReferenceValidatorInstance>.Empty;
        }

        var referenceValidatorProvider = new ValidatorProvider( validatorsBySymbol );

        await this._concurrentTaskRunner.RunInParallelAsync(
            initialCompilation.PartialCompilation.SyntaxTrees.Values,
            ( syntaxTree, visitor ) => visitor.Visit( syntaxTree ),
            () => new ReferenceValidationVisitor(
                this._serviceProvider,
                diagnosticAdder,
                referenceValidatorProvider,
                initialCompilation,
                cancellationToken ),
            cancellationToken );

        return validatorsBySymbol.Where( s => s.Key.GetResultingAccessibility() != AccessibilityFlags.SameType )
            .SelectMany( s => s )
            .ToImmutableArray();
    }

    public async Task<IEnumerable<ReferenceValidatorInstance>> GetReferenceValidatorsAsync(
        CompilationModel initialCompilation,
        UserDiagnosticSink diagnosticAdder,
        CancellationToken cancellationToken )
    {
        var validators = await this.GetValidatorsAsync(
            ValidatorKind.Reference,
            initialCompilation,
            CompilationModelVersion.Current,
            diagnosticAdder,
            cancellationToken );

        return validators.Cast<ReferenceValidatorInstance>();
    }

    private sealed class ValidatorProvider : IReferenceValidatorProvider
    {
        private readonly ImmutableDictionaryOfArray<ISymbol, ReferenceValidatorInstance> _validatorsBySymbol;

        public ValidatorProvider( ImmutableDictionaryOfArray<ISymbol, ReferenceValidatorInstance> validatorsBySymbol )
        {
            this._validatorsBySymbol = validatorsBySymbol;
            this.Properties = new ReferenceValidatorCollectionProperties( validatorsBySymbol.SelectMany( x => x ) );
        }

        public ReferenceValidatorCollectionProperties Properties { get; }

        public ImmutableArray<ReferenceValidatorInstance> GetValidators( ISymbol symbol ) => this._validatorsBySymbol[symbol];
    }
}