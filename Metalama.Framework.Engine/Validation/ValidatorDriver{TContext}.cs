// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities.UserCode;

namespace Metalama.Framework.Engine.Validation;

internal abstract class ValidatorDriver<TContext> : ValidatorDriver
{
    public abstract void Validate(
        ValidatorImplementation implementation,
        in TContext context,
        UserCodeInvoker invoker,
        UserCodeExecutionContext? executionContext );
}