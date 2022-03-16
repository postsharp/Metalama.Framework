// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.Validation;

internal class ValidatorDriver<TContext> : ValidatorDriver
{
    private readonly InvokeValidatorDelegate<TContext> _validateMethod;

    public ValidatorDriver( Type implementationType, MethodInfo validateMethodInfo, InvokeValidatorDelegate<TContext> validateMethod ) : base(
        implementationType,
        validateMethodInfo )
    {
        this._validateMethod = validateMethod;
    }

    public void Validate( ValidatorImplementation implementation, in TContext context, UserCodeInvoker invoker, UserCodeExecutionContext? executionContext )
    {
        var invokePayload = new InvokePayload( implementation, context, this );
        invoker.Invoke( InvokePayload.Validate, ref invokePayload, executionContext );
    }

    // Intentionally not marking the struct as readonly to avoid defensive copies when passing by ref.
    protected struct InvokePayload
    {
        private readonly ValidatorImplementation _implementation;
        private readonly TContext _context;
        private readonly ValidatorDriver<TContext> _driver;

        public InvokePayload( ValidatorImplementation implementation, in TContext context, ValidatorDriver<TContext> driver )
        {
            this._implementation = implementation;
            this._context = context;
            this._driver = driver;
        }

        public static bool Validate( ref InvokePayload payload )
        {
            payload._driver._validateMethod( payload._implementation.Implementation, payload._context );

            return true;
        }
    }
}