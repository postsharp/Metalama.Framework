using Metalama.Framework.Validation;
using System;

namespace Metalama.Framework.Engine.Validation;

internal class ReferenceValidatorDriver : ValidatorDriver
{
    private readonly InvokeReferenceValidatorDelegate _validateMethod;

    public ReferenceValidatorDriver( InvokeReferenceValidatorDelegate validateMethod )
    {
        this._validateMethod = validateMethod;
    }

    public void Validate( object instance, in ValidateReferenceContext context )
    {
        // TODO: use user code invoker.
        this._validateMethod.Invoke( instance, context );
    }
}