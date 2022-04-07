// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
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
        CompilationModelVersion compilationModelVersion,
        AspectPredecessor predecessor,
        MethodInfo method,
        Func<ProgrammaticValidatorSource, CompilationModel, IDiagnosticSink, IEnumerable<ValidatorInstance>> func )
    {
        if ( validatorKind != ValidatorKind.Reference )
        {
            throw new ArgumentOutOfRangeException( nameof(validatorKind) );
        }

        this.Driver = driverFactory.GetReferenceValidatorDriver( method );
        this.Kind = validatorKind;
        this.CompilationModelVersion = compilationModelVersion;
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
        this.Kind = validatorKind;
        this.CompilationModelVersion = compilationModelVersion;
        this.Predecessor = predecessor;
        this._func = func;
    }

    public IEnumerable<ValidatorInstance> GetValidators(
        ValidatorKind kind,
        CompilationModelVersion compilationModelVersion,
        CompilationModel compilation,
        IDiagnosticSink diagnosticAdder )
    {
        if ( kind == this.Kind && this.CompilationModelVersion == compilationModelVersion )
        {
            return this._func.Invoke( this, compilation, diagnosticAdder );
        }
        else
        {
            return Enumerable.Empty<ValidatorInstance>();
        }
    }

    public ValidatorKind Kind { get; }

    public CompilationModelVersion CompilationModelVersion { get; set; }
}