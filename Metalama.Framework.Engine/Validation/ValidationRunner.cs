// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.UserCode;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Validation;

internal sealed class ValidationRunner
{
    private readonly AspectPipelineConfiguration _configuration;
    private readonly ImmutableArray<IValidatorSource> _sources;
    private readonly CancellationToken _cancellationToken;
    private readonly ProjectServiceProvider _serviceProvider;
    private readonly UserCodeInvoker _userCodeInvoker;

    public ValidationRunner( AspectPipelineConfiguration configuration, ImmutableArray<IValidatorSource> sources, CancellationToken cancellationToken )
    {
        this._configuration = configuration;
        this._sources = sources;
        this._serviceProvider = configuration.ServiceProvider;
        this._userCodeInvoker = this._serviceProvider.GetRequiredService<UserCodeInvoker>();
        this._cancellationToken = cancellationToken;
    }

    /// <summary>
    /// Runs both declaration and reference validators.
    /// </summary>
    public ValidationResult RunAll( CompilationModel initialCompilation, CompilationModel finalCompilation )
    {
        var initialCompilationWithEnhancements = initialCompilation.WithAspectRepository( finalCompilation.AspectRepository, "With final aspect repository" );

        var userDiagnosticSink = new UserDiagnosticSink( this._configuration.CompileTimeProject, this._configuration.CodeFixFilter );
        this.RunDeclarationValidators( initialCompilationWithEnhancements, finalCompilation, userDiagnosticSink );

        var transitiveValidators = this.RunReferenceValidators( initialCompilationWithEnhancements, userDiagnosticSink );

        return new ValidationResult( transitiveValidators, userDiagnosticSink.ToImmutable() );
    }

    public void RunDeclarationValidators( CompilationModel initialCompilation, CompilationModel finalCompilation, UserDiagnosticSink diagnosticAdder )
    {
        this.RunDeclarationValidators( initialCompilation, CompilationModelVersion.Initial, diagnosticAdder );
        this.RunDeclarationValidators( finalCompilation, CompilationModelVersion.Final, diagnosticAdder );
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
                userCodeExecutionContext.InvokedMember = validator.Driver.UserCodeMemberInfo;

                validator.Validate( diagnosticAdder, this._userCodeInvoker, userCodeExecutionContext );
            }
        }
    }

    private ImmutableArray<ReferenceValidatorInstance> RunReferenceValidators( CompilationModel initialCompilation, UserDiagnosticSink diagnosticAdder )
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

        using ( var visitor = new ReferenceValidationVisitor(
                   this._serviceProvider,
                   diagnosticAdder,
                   s => validatorsBySymbol[s],
                   initialCompilation,
                   this._cancellationToken ) )
        {
            foreach ( var syntaxTree in initialCompilation.PartialCompilation.SyntaxTrees )
            {
                visitor.Visit( syntaxTree.Value );
            }
        }

        return validatorsBySymbol.Where( s => s.Key.GetResultingAccessibility() != AccessibilityFlags.SameType )
            .SelectMany( s => s )
            .ToImmutableArray();
    }

    public IEnumerable<ReferenceValidatorInstance> GetReferenceValidators( CompilationModel initialCompilation, UserDiagnosticSink diagnosticAdder )
        => this._sources
            .SelectMany( s => s.GetValidators( ValidatorKind.Reference, CompilationModelVersion.Current, initialCompilation, diagnosticAdder ) )
            .Cast<ReferenceValidatorInstance>();
}