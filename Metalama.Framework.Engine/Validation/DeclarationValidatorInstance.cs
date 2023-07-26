// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Engine.Validation;

internal sealed class DeclarationValidatorInstance : ValidatorInstance
{
    private readonly ValidatorDriver<DeclarationValidationContext> _driver;

    public DeclarationValidatorInstance(
        IDeclaration validatedDeclaration,
        ValidatorDriver<DeclarationValidationContext> driver,
        in ValidatorImplementation implementation,
        string description ) : base(
        validatedDeclaration,
        driver,
        implementation,
        description )
    {
        this._driver = driver;
    }

    public void Validate( IDiagnosticSink diagnosticAdder, UserCodeInvoker userCodeInvoker, UserCodeExecutionContext userCodeExecutionContext )
    {
        var validationContext = new DeclarationValidationContext(
            this.ValidatedDeclaration,
            this.Implementation.State,
            diagnosticAdder,
            this );

        this._driver.Validate( this.Implementation, validationContext, userCodeInvoker, userCodeExecutionContext );
    }
}