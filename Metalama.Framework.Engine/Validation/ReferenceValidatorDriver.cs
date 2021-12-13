// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Validation;

namespace Metalama.Framework.Engine.Validation;

internal class ReferenceValidatorDriver : ValidatorDriver
{
    private readonly InvokeReferenceValidatorDelegate _validateMethod;

    public ReferenceValidatorDriver( InvokeReferenceValidatorDelegate validateMethod )
    {
        this._validateMethod = validateMethod;
    }

    public void Validate( object instance, in ReferenceValidationContext context )
    {
        // TODO: use user code invoker.
        this._validateMethod.Invoke( instance, context );
    }
}