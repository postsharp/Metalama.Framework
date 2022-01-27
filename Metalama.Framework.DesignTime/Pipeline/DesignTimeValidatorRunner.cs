// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.DesignTime.Pipeline;

internal class DesignTimeValidatorRunner
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CompilationPipelineResult _compilationResult;
    private readonly IProject _project;
    private readonly DesignTimeAspectPipeline _pipeline;
    private readonly Dictionary<ISymbol, ImmutableArray<ReferenceValidatorInstance>> _validators = new();

    public DesignTimeValidatorRunner(
        IServiceProvider serviceProvider,
        CompilationPipelineResult compilationResult,
        IProject project,
        DesignTimeAspectPipeline pipeline )
    {
        this._serviceProvider = serviceProvider;
        this._compilationResult = compilationResult;
        this._project = project;
        this._pipeline = pipeline;
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

        // Perform additional analysis not done by the design-time pipeline.
        // We do it from here so that we benefit from caching.
        TemplatingCodeValidator.Validate(
            this._serviceProvider,
            model,
            diagnosticSink.Report,
            this._pipeline.IsCompileTimeSyntaxTreeOutdated( model.SyntaxTree.FilePath ),
            true,
            cancellationToken );
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