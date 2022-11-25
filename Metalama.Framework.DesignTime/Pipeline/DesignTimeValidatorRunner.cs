// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal class DesignTimeValidatorRunner
{
    private readonly ProjectServiceProvider _serviceProvider;
    private readonly CompilationPipelineResult _compilationResult;
    private readonly IProject _project;
    private readonly Dictionary<ISymbol, ImmutableArray<ReferenceValidatorInstance>> _validators = new();

    public DesignTimeValidatorRunner(
        ProjectServiceProvider serviceProvider,
        CompilationPipelineResult compilationResult,
        IProject project )
    {
        this._serviceProvider = serviceProvider;
        this._compilationResult = compilationResult;
        this._project = project;
    }

    public void Validate( SemanticModel model, UserDiagnosticSink diagnosticSink, CancellationToken cancellationToken )
    {
        if ( !this._compilationResult.Validators.IsEmpty )
        {
            var compilation = CompilationModel.CreateInitialInstance( this._project, PartialCompilation.CreatePartial( model.Compilation, model.SyntaxTree ) );

            using var visitor = new ReferenceValidationVisitor(
                this._serviceProvider,
                diagnosticSink,
                s => this.GetValidatorsForSymbol( s, compilation ),
                compilation,
                cancellationToken );

            visitor.Visit( model );
        }
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
                .SelectImmutableArray( x => x.ToReferenceValidationInstance( compilation ) );

            this._validators[symbol] = validators;
        }

        return validators;
    }
}