// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.Validation;

/// <summary>
/// Validation drivers cache and execute the method call to the user validation method.
/// </summary>
internal abstract class ValidatorDriver
{
    /// <summary>
    /// Gets the type defining the validation method.
    /// </summary>
    public Type ImplementationType { get; }

    /// <summary>
    /// Gets the name of the validation method.
    /// </summary>
    public MethodInfo ValidateMethod { get; }

    protected ValidatorDriver( Type implementationType, MethodInfo validateMethod )
    {
        this.ImplementationType = implementationType;
        this.ValidateMethod = validateMethod;
    }
}

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