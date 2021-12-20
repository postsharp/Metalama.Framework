// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Engine.Validation;

internal class DeclarationValidatorInstance : ValidatorInstance
{
    private readonly DeclarationValidatorDriver _driver;

    public DeclarationValidatorInstance( IDeclaration validatedDeclaration, ValidatorDriver driver, in ValidatorImplementation implementation ) : base(
        validatedDeclaration,
        driver,
        implementation )
    {
        this._driver = (DeclarationValidatorDriver) driver;
    }

    public void Validate( IDiagnosticSink diagnosticAdder )
    {
        this._driver.Validate( this.Implementation, this.ValidatedDeclaration, diagnosticAdder );
    }
}