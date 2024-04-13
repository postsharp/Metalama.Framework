// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Validation;

internal sealed class ProgrammaticValidatorSource : IValidatorSource
{
    public ValidatorDriver Driver { get; }

    public AspectPredecessor Predecessor { get; }

    private readonly Func<ProgrammaticValidatorSource, CompilationModel, AspectResultCollector, CancellationToken, Task> _addValidatorsAction;
    private readonly ValidatorKind _kind;
    private readonly CompilationModelVersion _compilationModelVersion;

    public ProgrammaticValidatorSource(
        ValidatorDriver driver,
        ValidatorKind validatorKind,
        CompilationModelVersion compilationModelVersion,
        AspectPredecessor predecessor,
        Func<ProgrammaticValidatorSource, CompilationModel, AspectResultCollector, CancellationToken, Task> addValidatorsAction )
    {
        if ( validatorKind != ValidatorKind.Reference )
        {
            throw new ArgumentOutOfRangeException( nameof(validatorKind) );
        }

        this.Driver = driver;
        this._kind = validatorKind;
        this._compilationModelVersion = compilationModelVersion;
        this.Predecessor = predecessor;
        this._addValidatorsAction = addValidatorsAction;
    }

    public ProgrammaticValidatorSource(
        IValidatorDriverFactory driverFactory,
        ValidatorKind validatorKind,
        CompilationModelVersion compilationModelVersion,
        AspectPredecessor predecessor,
        ValidatorDelegate<DeclarationValidationContext> method,
        Func<ProgrammaticValidatorSource, CompilationModel, AspectResultCollector, CancellationToken, Task> addValidatorsAction )
    {
        if ( validatorKind != ValidatorKind.Definition )
        {
            throw new ArgumentOutOfRangeException( nameof(validatorKind) );
        }

        this.Driver = driverFactory.GetDeclarationValidatorDriver( method );
        this._kind = validatorKind;
        this._compilationModelVersion = compilationModelVersion;
        this.Predecessor = predecessor;
        this._addValidatorsAction = addValidatorsAction;
    }

    public Task AddValidatorsAsync(
        ValidatorKind kind,
        CompilationModelVersion compilationModelVersion,
        CompilationModel compilation,
        AspectResultCollector collector,
        CancellationToken cancellationToken )
    {
        if ( kind == this._kind && this._compilationModelVersion == compilationModelVersion )
        {
            return this._addValidatorsAction.Invoke( this, compilation, collector, cancellationToken );
        }
        else
        {
            return Task.CompletedTask;
        }
    }
}