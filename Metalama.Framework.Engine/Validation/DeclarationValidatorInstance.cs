// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Engine.Validation;

internal class DeclarationValidatorInstance : ValidatorInstance
{
    private readonly ValidatorDriver<DeclarationValidationContext> _driver;

    public DeclarationValidatorInstance(
        IDeclaration validatedDeclaration,
        ValidatorDriver<DeclarationValidationContext> driver,
        in ValidatorImplementation implementation ) : base(
        validatedDeclaration,
        driver,
        implementation )
    {
        this._driver = driver;
    }

    public void Validate( IDiagnosticSink diagnosticAdder, UserCodeInvoker userCodeInvoker, UserCodeExecutionContext? userCodeExecutionContext )
    {
        var validationContext = new DeclarationValidationContext(
            this.ValidatedDeclaration,
            this.Implementation.State,
            diagnosticAdder );

        this._driver.Validate( this.Implementation, validationContext, userCodeInvoker, userCodeExecutionContext );
    }
}