// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
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

    public void Validate( CompilationModel initialCompilation, CompilationModel finalCompilation, IDiagnosticSink diagnosticAdder )
    {
        this.RunDeclarationValidators( finalCompilation, diagnosticAdder );
    }

    private void RunDeclarationValidators( CompilationModel finalCompilation, IDiagnosticSink diagnosticAdder )
    {
        var validators = this._sources
            .Where( s => s.Kind == ValidatorKind.Definition )
            .SelectMany( s => s.GetValidators( finalCompilation, diagnosticAdder ) ).Cast<DeclarationValidatorInstance>();

        foreach ( var validator in validators )
        {
            validator.Validate( diagnosticAdder );
        }
    }


    private void RunReferenceValidators( CompilationModel initialCompilation, IDiagnosticSink diagnosticAdder )
    {
        var validatorsBySymbol = this._sources.Where( s => s.Kind == ValidatorKind.Reference )
            .SelectMany( s => s.GetValidators( initialCompilation, diagnosticAdder ) )
            .Cast<ReferenceValidatorInstance>()
            .ToMultiValueDictionary(
                v => v.ValidatedDeclaration.GetSymbol().AssertNotNull(),
                v => v );

        var visitor = new Visitor( diagnosticAdder, validatorsBySymbol, initialCompilation );

        foreach ( var syntaxTree in initialCompilation.PartialCompilation.SyntaxTrees )
        {
            visitor.Visit( syntaxTree.Value );
        }

    }

}