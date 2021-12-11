// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Validation;

internal readonly struct ValidatorSource
{
    private readonly Func<CompilationModel, IDiagnosticAdder, IEnumerable<ValidatorInstance>> _func;

    public ValidatorKind Kind { get; }

    public ValidatorSource( Func<CompilationModel, IDiagnosticAdder, IEnumerable<ValidatorInstance>> func, ValidatorKind kind )
    {
        this._func = func;
        this.Kind = kind;
    }

    public IEnumerable<ValidatorInstance> GetValidators( CompilationModel compilation, IDiagnosticAdder diagnosticAdder )
        => this._func.Invoke( compilation, diagnosticAdder );
}

internal enum ValidatorKind
{
    Definition,
    Reference
}