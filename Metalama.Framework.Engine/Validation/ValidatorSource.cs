// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Validation;

internal class ValidatorSource
{
    public ValidatorDriver Driver { get; }

    public AspectPredecessor Predecessor { get; }

    public string MethodName { get; }

    private readonly Func<ValidatorSource, CompilationModel, IDiagnosticSink, IEnumerable<ValidatorInstance>> _func;

    public ValidatorKind Kind { get; }

    public ValidatorSource( IValidatorDriverFactory driverFactory, AspectPredecessor predecessor, string methodName,  ValidatorKind kind, Func<ValidatorSource, CompilationModel, IDiagnosticSink, IEnumerable<ValidatorInstance>> func )
    {
        this.Driver = driverFactory.GetValidatorDriver( methodName, kind );
        this.Predecessor = predecessor;
        this.MethodName = methodName;
        this._func = func;
        this.Kind = kind;
    }

    public IEnumerable<ValidatorInstance> GetValidators( CompilationModel compilation, IDiagnosticSink diagnosticAdder )
        => this._func.Invoke( this, compilation, diagnosticAdder );
}

internal enum ValidatorKind
{
    Definition,
    Reference
}