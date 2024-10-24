// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Validation;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Validation;

public sealed class ReferenceValidatorInstance : ValidatorInstance
{
    public ReferenceValidatorInstance(
        IDeclaration validatedDeclaration,
        ValidatorDriver driver,
        ValidatorImplementation implementation,
        ReferenceKinds referenceKinds,
        bool includeDerivedTypes,
        string description,
        ReferenceGranularity granularity ) : base( validatedDeclaration, driver, implementation, description )
    {
        this.Properties = new ReferenceValidatorProperties( validatedDeclaration, referenceKinds, includeDerivedTypes );
        this.Granularity = granularity;
    }

    public ReferenceGranularity Granularity { get; }

    public IReferenceValidatorProperties Properties { get; }

    internal void Validate(
        IDeclaration referencingDeclaration,
        IDiagnosticSink diagnosticAdder,
        UserCodeInvoker userCodeInvoker,
        UserCodeExecutionContext userCodeExecutionContext,
        IEnumerable<ReferencingSymbolInfo> references )
    {
        var validationContext = new ReferenceValidationContextImpl(
            this,
            this.ValidatedDeclaration,
            referencingDeclaration,
            this.Implementation.State,
            diagnosticAdder,
            references );

        ((ValidatorDriver<ReferenceValidationContext>) this.Driver).Validate(
            this.Implementation,
            validationContext,
            userCodeInvoker,
            userCodeExecutionContext );
    }
}