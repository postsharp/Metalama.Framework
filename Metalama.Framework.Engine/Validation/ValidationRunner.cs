// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Validation;

internal partial class ValidationRunner
{
    private readonly ImmutableArray<ValidatorSource> _sources;

    public ValidationRunner( ImmutableArray<ValidatorSource> sources )
    {
        this._sources = sources;
    }

    public IReadOnlyList<ReferenceValidatorInstance> Validate( CompilationModel initialCompilation, CompilationModel finalCompilation, IDiagnosticSink diagnosticAdder )
    {
        this.RunDeclarationValidators( finalCompilation, diagnosticAdder );
        return this.RunReferenceValidators( initialCompilation, diagnosticAdder );
    }

    private void RunDeclarationValidators( CompilationModel finalCompilation, IDiagnosticSink diagnosticAdder )
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

    private IReadOnlyList<ReferenceValidatorInstance> RunReferenceValidators( CompilationModel initialCompilation, IDiagnosticSink diagnosticAdder )
    {
        var validators = this._sources.Where( s => s.Kind == ValidatorKind.Reference )
            .SelectMany( s => s.GetValidators( initialCompilation, diagnosticAdder ) )
            .Cast<ReferenceValidatorInstance>();

        var validatorsBySymbol = validators
            .ToMultiValueDictionary(
                v => v.ValidatedDeclaration.GetSymbol().AssertNotNull(),
                v => v );

        var visitor = new Visitor( diagnosticAdder, validatorsBySymbol, initialCompilation );

        foreach ( var syntaxTree in initialCompilation.PartialCompilation.SyntaxTrees )
        {
            visitor.Visit( syntaxTree.Value );
        }

        return validatorsBySymbol.Where( s => s.Key.GetResultingAccessibility() != AccessibilityFlags.SameType )
            .SelectMany( s => s )
            .ToList();
    }
}

