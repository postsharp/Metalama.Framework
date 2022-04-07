// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Validation;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.Validation;

internal class ReferenceValidatorDriver : ValidatorDriver<ReferenceValidationContext>
{
    private readonly InvokeValidatorDelegate<ReferenceValidationContext> _validateMethod;
    private readonly MethodInfo _validateMethodInfo;

    public ReferenceValidatorDriver(
        Type implementationType,
        MethodInfo validateMethodInfoInfo,
        InvokeValidatorDelegate<ReferenceValidationContext> validateMethod )
    {
        this.ImplementationType = implementationType;
        this._validateMethodInfo = validateMethodInfoInfo;
        this._validateMethod = validateMethod;
    }

    /// <summary>
    /// Gets the type defining the validation method.
    /// </summary>
    public Type ImplementationType { get; }

    public override void Validate(
        ValidatorImplementation implementation,
        in ReferenceValidationContext context,
        UserCodeInvoker invoker,
        UserCodeExecutionContext? executionContext )
    {
        var invokePayload = new InvokePayload( implementation, context, this );
        invoker.Invoke( InvokePayload.Validate, ref invokePayload, executionContext );
    }

    // Intentionally not marking the struct as readonly to avoid defensive copies when passing by ref.
    protected struct InvokePayload
    {
        private readonly ValidatorImplementation _implementation;
        private readonly ReferenceValidationContext _context;
        private readonly ReferenceValidatorDriver _driver;

        public InvokePayload( ValidatorImplementation implementation, in ReferenceValidationContext context, ReferenceValidatorDriver driver )
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

    internal override UserCodeMemberInfo UserCodeMemberInfo => UserCodeMemberInfo.FromMemberInfo( this._validateMethodInfo );

    public string MethodName => this._validateMethodInfo.Name;
}