// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
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

        return new ValidationResult( await referenceValidatorsTask, userDiagnosticSink.ToImmutable() );
    }

    public void RunDeclarationValidators( CompilationModel compilation, CompilationModelVersion version, UserDiagnosticSink diagnosticAdder )
    {
        var validators = this._sources
            .SelectMany( s => s.GetValidators( ValidatorKind.Definition, version, compilation, diagnosticAdder ) )
            .Cast<DeclarationValidatorInstance>();

        var userCodeExecutionContext = new UserCodeExecutionContext( this._serviceProvider, diagnosticAdder, default, compilationModel: compilation );

        using ( UserCodeExecutionContext.WithContext( userCodeExecutionContext ) )
        {
            foreach ( var validator in validators )
            {
                userCodeExecutionContext.Description = validator.Driver.GetUserCodeMemberInfo( validator );

                validator.Validate( diagnosticAdder, this._userCodeInvoker, userCodeExecutionContext );
            }
        }
    }

    public async Task RunDeclarationValidatorsAsync(
        CompilationModel initialCompilation,
        CompilationModel finalCompilation,
        UserDiagnosticSink diagnosticAdder,
        CancellationToken cancellationToken )
    {
        var task1 = this.RunDeclarationValidatorsAsync( initialCompilation, CompilationModelVersion.Initial, diagnosticAdder, cancellationToken );
        var task2 = this.RunDeclarationValidatorsAsync( finalCompilation, CompilationModelVersion.Final, diagnosticAdder, cancellationToken );

        await Task.WhenAll( task1, task2 );
    }

    private async Task RunDeclarationValidatorsAsync(
        CompilationModel compilation,
        CompilationModelVersion version,
        UserDiagnosticSink diagnosticAdder,
        CancellationToken cancellationToken )
    {
        var validators = this._sources
            .SelectMany( s => s.GetValidators( ValidatorKind.Definition, version, compilation, diagnosticAdder ) )
            .Cast<DeclarationValidatorInstance>();

        var userCodeExecutionContext = new UserCodeExecutionContext( this._serviceProvider, diagnosticAdder, default, compilationModel: compilation );

        await this._concurrentTaskRunner.RunInParallelAsync( validators, RunValidator, cancellationToken );

        void RunValidator( DeclarationValidatorInstance validator )
        {
            using ( UserCodeExecutionContext.WithContext( userCodeExecutionContext ) )
            {
                userCodeExecutionContext.Description = validator.Driver.GetUserCodeMemberInfo( validator );

                validator.Validate( diagnosticAdder, this._userCodeInvoker, userCodeExecutionContext );
            }
        }
    }

    private async Task<ImmutableArray<ReferenceValidatorInstance>> RunReferenceValidatorsAsync(
        CompilationModel initialCompilation,
        UserDiagnosticSink diagnosticAdder,
        CancellationToken cancellationToken )
    {
        var validators = this.GetReferenceValidators( initialCompilation, diagnosticAdder );

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

    public IEnumerable<ReferenceValidatorInstance> GetReferenceValidators( CompilationModel initialCompilation, UserDiagnosticSink diagnosticAdder )
        => this._sources
            .SelectMany( s => s.GetValidators( ValidatorKind.Reference, CompilationModelVersion.Current, initialCompilation, diagnosticAdder ) )
            .Cast<ReferenceValidatorInstance>();

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