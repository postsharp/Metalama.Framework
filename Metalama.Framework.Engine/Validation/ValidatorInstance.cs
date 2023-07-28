// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Metalama.Framework.Engine.Validation;

public abstract class ValidatorInstance : IDiagnosticSource
{
    public ValidatorDriver Driver { get; }

    public IDeclaration ValidatedDeclaration { get; }

    public ValidatorImplementation Implementation { get; }

    protected ValidatorInstance( IDeclaration validatedDeclaration, ValidatorDriver driver, in ValidatorImplementation implementation, string description )
    {
        this.Driver = driver;
        this.Implementation = implementation;
        this.ValidatedDeclaration = validatedDeclaration;
        this.DiagnosticSourceDescription = description;
    }

    public string DiagnosticSourceDescription { get; }

    public override string ToString() => $"{this.GetType().Name}: {this.DiagnosticSourceDescription}";
}