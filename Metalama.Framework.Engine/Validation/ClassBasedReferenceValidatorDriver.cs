// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Validation;
using System;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.Validation;

internal sealed class ClassBasedReferenceValidatorDriver : ValidatorDriver<ReferenceValidationContext>
{
    public ClassBasedReferenceValidatorDriver( Type validatorType )
    {
        var baseMethod = typeof(ReferenceValidator).GetMethod( nameof(ReferenceValidator.Validate) ).AssertNotNull();

        var method = validatorType.GetMethod(
                baseMethod.Name,
                BindingFlags.Instance | BindingFlags.Public,
                null,
                baseMethod.GetParameters().SelectAsArray( t => t.ParameterType ),
                null )
            .AssertNotNull();

        this.UserCodeMemberInfo = UserCodeMemberInfo.FromMemberInfo( method );
    }

    public override void Validate(
        ValidatorImplementation implementation,
        in ReferenceValidationContext context,
        UserCodeInvoker invoker,
        UserCodeExecutionContext executionContext )
    {
        var invokePayload = new InvokePayload( implementation, context );
        invoker.Invoke( InvokePayload.Validate, ref invokePayload, executionContext );
    }

    internal override UserCodeMemberInfo UserCodeMemberInfo { get; }

    public override string? MethodName => null;

    private struct InvokePayload
    {
        private readonly ValidatorImplementation _implementation;
        private readonly ReferenceValidationContext _context;

        public InvokePayload( ValidatorImplementation implementation, ReferenceValidationContext context )
        {
            this._implementation = implementation;
            this._context = context;
        }

        public static bool Validate( ref InvokePayload payload )
        {
            ((ReferenceValidator) payload._implementation.Implementation).Validate( payload._context );

            return true;
        }
    }
}