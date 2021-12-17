// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Validation;
using System;

namespace Metalama.Framework.Engine.Validation;

internal class ReferenceValidatorDriver : ValidatorDriver
{
    private readonly InvokeReferenceValidatorDelegate _validateMethod;

    public ReferenceValidatorDriver( Type implementationType, string methodName, InvokeReferenceValidatorDelegate validateMethod ) : base(
        implementationType,
        methodName )
    {
        this._validateMethod = validateMethod;
    }

    public void Validate( ValidatorImplementation implementation, in ReferenceValidationContext context )
    {
        // TODO: use user code invoker.
        this._validateMethod.Invoke( implementation.Implementation, context );
    }
}