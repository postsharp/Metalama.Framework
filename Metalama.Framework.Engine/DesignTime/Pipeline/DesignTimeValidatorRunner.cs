// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.DesignTime.Pipeline;

internal class DesignTimeValidatorRunner
{
    private readonly CompilationPipelineResult _compilationResult;
    private readonly IProject _project;
    private readonly Dictionary<ISymbol, ImmutableArray<ReferenceValidatorInstance>> _validators = new();

    public DesignTimeValidatorRunner( CompilationPipelineResult compilationResult, IProject project )
    {
        this._compilationResult = compilationResult;
        this._project = project;
    }

    public void Validate( SemanticModel model, UserDiagnosticSink diagnosticSink, CancellationToken cancellationToken )
    {
        var compilation = CompilationModel.CreateInitialInstance( this._project, PartialCompilation.CreatePartial( model.Compilation, model.SyntaxTree ) );

        var visitor = new ReferenceValidationVisitor( diagnosticSink, s => this.GetValidatorsForSymbol( s, compilation ), compilation, cancellationToken );
        visitor.Visit( model );
    }

    private ImmutableArray<ReferenceValidatorInstance> GetValidatorsForSymbol( ISymbol symbol, CompilationModel compilation )
    {
        if ( this._validators.TryGetValue( symbol, out var validators ) )
        {
            return validators;
        }
        else
        {
            validators = this._compilationResult.Validators.GetValidatorsForSymbol( symbol )
                .Select( x => x.ToReferenceValidationInstance( compilation ) )
                .ToImmutableArray();

            this._validators[symbol] = validators;
        }

        return validators;
    }
}