// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Metalama.Framework.Engine.Validation;

internal class ProgrammaticValidatorSource : IValidatorSource
{
    public ValidatorDriver Driver { get; }

    public AspectPredecessor Predecessor { get; }

    private readonly Func<ProgrammaticValidatorSource, CompilationModel, IDiagnosticSink, IEnumerable<ValidatorInstance>> _func;

    public ProgrammaticValidatorSource(
        IValidatorDriverFactory driverFactory,
        ValidatorKind validatorKind,
        AspectPredecessor predecessor,
        MethodInfo method,
        Func<ProgrammaticValidatorSource, CompilationModel, IDiagnosticSink, IEnumerable<ValidatorInstance>> func )
    {
        this.Driver = driverFactory.GetValidatorDriver( method, validatorKind );
        this.Kind = validatorKind;
        this.Predecessor = predecessor;
        this._func = func;
    }

    public IEnumerable<ValidatorInstance> GetValidators( CompilationModel compilation, IDiagnosticSink diagnosticAdder )
        => this._func.Invoke( this, compilation, diagnosticAdder );

    public ValidatorKind Kind { get; }
}