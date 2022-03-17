// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Validation;

internal class ValidationRunner
{
    private readonly AspectPipelineConfiguration _configuration;
    private readonly ImmutableArray<IValidatorSource> _sources;
    private readonly CancellationToken _cancellationToken;
    private readonly IServiceProvider _serviceProvider;
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
        var userDiagnosticSink = new UserDiagnosticSink( this._configuration.CompileTimeProject, this._configuration.CodeFixFilter );
        this.RunDeclarationValidators( finalCompilation, userDiagnosticSink );

        var transitiveValidators = this.RunReferenceValidators( initialCompilation, userDiagnosticSink );

        return new ValidationResult( transitiveValidators, userDiagnosticSink.ToImmutable() );
    }

    public void RunDeclarationValidators( CompilationModel finalCompilation, UserDiagnosticSink diagnosticAdder )
    {
        var validators = this._sources
            .Where( s => s.Kind == ValidatorKind.Definition )
            .SelectMany( s => s.GetValidators( finalCompilation, diagnosticAdder ) )
            .Cast<DeclarationValidatorInstance>();

        var userCodeExecutionContext = new UserCodeExecutionContext( this._serviceProvider, diagnosticAdder, default, compilationModel: finalCompilation );

        using ( UserCodeExecutionContext.WithContext( userCodeExecutionContext ) )
        {
            foreach ( var validator in validators )
            {
                userCodeExecutionContext.InvokedMember = UserCodeMemberInfo.FromMemberInfo( validator.Driver.ValidateMethod );

                validator.Validate( diagnosticAdder, this._userCodeInvoker, null );
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

    public IEnumerable<ReferenceValidatorInstance> GetReferenceValidators( CompilationModel initialCompilation, IDiagnosticSink diagnosticAdder )
        => this._sources.Where( s => s.Kind == ValidatorKind.Reference )
            .SelectMany( s => s.GetValidators( initialCompilation, diagnosticAdder ) )
            .Cast<ReferenceValidatorInstance>();
}