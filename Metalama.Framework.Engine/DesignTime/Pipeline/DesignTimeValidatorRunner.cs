using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
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
    private readonly CompilationResult _compilationResult;
    private readonly IProject _project;
    private readonly Dictionary<ISymbol, ImmutableArray<ReferenceValidatorInstance>> _validators = new();

    public DesignTimeValidatorRunner( CompilationResult compilationResult, IProject project )
    {
        this._compilationResult = compilationResult;
        this._project = project;
    }

    public ImmutableUserDiagnosticList Validate( SemanticModel model, CancellationToken cancellationToken )
    {
        var diagnostics = new UserDiagnosticSink();
        
        // TODO: We may want to optimize here:
        //  1. We may have created a compilation model upstream.
        //  2. PartialCompilation.CreatePartial creates a closure, but we don't need it.
        var compilation = CompilationModel.CreateInitialInstance( this._project, PartialCompilation.CreatePartial( model.Compilation, model.SyntaxTree ) );
        
        var visitor = new ReferenceValidationVisitor( diagnostics, s => this.GetValidatorsForSymbol(  s, compilation ), compilation, cancellationToken );
        visitor.Visit( model );

        return diagnostics.ToImmutable();
    }

    private ImmutableArray<ReferenceValidatorInstance> GetValidatorsForSymbol( ISymbol symbol, CompilationModel compilation )
    {
        if ( this._validators.TryGetValue( symbol, out var validators ) )
        {
            return validators;
        }
        else
        {
            validators = this._compilationResult.GetValidatorsForSymbol( symbol )
                .Select( x => x.ToReferenceValidationInstance( compilation ) )
                .ToImmutableArray();

            this._validators[symbol] = validators;
        }

        return validators;
    }
}