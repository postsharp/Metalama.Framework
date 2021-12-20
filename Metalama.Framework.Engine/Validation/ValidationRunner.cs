// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
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

    public ValidationRunner( AspectPipelineConfiguration configuration, ImmutableArray<IValidatorSource> sources, CancellationToken cancellationToken )
    {
        this._configuration = configuration;
        this._sources = sources;
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

    public void RunDeclarationValidators( CompilationModel finalCompilation, IDiagnosticSink diagnosticAdder )
    {
        var validators = this._sources
            .Where( s => s.Kind == ValidatorKind.Definition )
            .SelectMany( s => s.GetValidators( finalCompilation, diagnosticAdder ) )
            .Cast<DeclarationValidatorInstance>();

        foreach ( var validator in validators )
        {
            validator.Validate( diagnosticAdder );
        }
    }

    private ImmutableArray<ReferenceValidatorInstance> RunReferenceValidators( CompilationModel initialCompilation, IDiagnosticSink diagnosticAdder )
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

        var visitor = new ReferenceValidationVisitor( diagnosticAdder, s => validatorsBySymbol[s], initialCompilation, this._cancellationToken );

        foreach ( var syntaxTree in initialCompilation.PartialCompilation.SyntaxTrees )
        {
            visitor.Visit( syntaxTree.Value );
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