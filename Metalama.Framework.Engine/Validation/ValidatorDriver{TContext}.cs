// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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