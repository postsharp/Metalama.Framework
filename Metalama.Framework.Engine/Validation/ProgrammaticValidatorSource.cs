// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Validation;

internal sealed class ProgrammaticValidatorSource : IValidatorSource
{
    public ValidatorDriver Driver { get; }

    public AspectPredecessor Predecessor { get; }

    private readonly Func<ProgrammaticValidatorSource, CompilationModel, IDiagnosticSink, IEnumerable<ValidatorInstance>> _func;
    private readonly ValidatorKind _kind;
    private readonly CompilationModelVersion _compilationModelVersion;

    public ProgrammaticValidatorSource(
        ValidatorDriver driver,
        ValidatorKind validatorKind,
        CompilationModelVersion compilationModelVersion,
        AspectPredecessor predecessor,
        Func<ProgrammaticValidatorSource, CompilationModel, IDiagnosticSink, IEnumerable<ValidatorInstance>> func )
    {
        if ( validatorKind != ValidatorKind.Reference )
        {
            throw new ArgumentOutOfRangeException( nameof(validatorKind) );
        }

        this.Driver = driver;
        this._kind = validatorKind;
        this._compilationModelVersion = compilationModelVersion;
        this.Predecessor = predecessor;
        this._func = func;
    }

    public ProgrammaticValidatorSource(
        IValidatorDriverFactory driverFactory,
        ValidatorKind validatorKind,
        CompilationModelVersion compilationModelVersion,
        AspectPredecessor predecessor,
        ValidatorDelegate<DeclarationValidationContext> method,
        Func<ProgrammaticValidatorSource, CompilationModel, IDiagnosticSink, IEnumerable<ValidatorInstance>> func )
    {
        if ( validatorKind != ValidatorKind.Definition )
        {
            throw new ArgumentOutOfRangeException( nameof(validatorKind) );
        }

        this.Driver = driverFactory.GetDeclarationValidatorDriver( method );
        this._kind = validatorKind;
        this._compilationModelVersion = compilationModelVersion;
        this.Predecessor = predecessor;
        this._func = func;
    }

    public IEnumerable<ValidatorInstance> GetValidators(
        ValidatorKind kind,
        CompilationModelVersion compilationModelVersion,
        CompilationModel compilation,
        UserDiagnosticSink diagnosticAdder )
    {
        if ( kind == this._kind && this._compilationModelVersion == compilationModelVersion )
        {
            return this._func.Invoke( this, compilation, diagnosticAdder );
        }
        else
        {
            return Enumerable.Empty<ValidatorInstance>();
        }
    }
}