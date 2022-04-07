// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Engine.Validation;

internal class DeclarationValidatorDriver : ValidatorDriver<DeclarationValidationContext>
{
    private readonly ValidatorDelegate<DeclarationValidationContext> _validator;

    public DeclarationValidatorDriver( ValidatorDelegate<DeclarationValidationContext> validator )
    {
        this._validator = validator;
    }

    public override void Validate(
        ValidatorImplementation implementation,
        in DeclarationValidationContext context,
        UserCodeInvoker invoker,
        UserCodeExecutionContext? executionContext )
    {
        var invokePayload = new InvokePayload( context, this );
        invoker.Invoke( InvokePayload.Validate, ref invokePayload, executionContext );
    }

    // Intentionally not marking the struct as readonly to avoid defensive copies when passing by ref.
    protected struct InvokePayload
    {
        private readonly DeclarationValidationContext _context;
        private readonly DeclarationValidatorDriver _driver;

        public InvokePayload( in DeclarationValidationContext context, DeclarationValidatorDriver driver )
        {
            this._context = context;
            this._driver = driver;
        }

        public static bool Validate( ref InvokePayload payload )
        {
            payload._driver._validator.Invoke( payload._context );

            return true;
        }
    }

    internal override UserCodeMemberInfo UserCodeMemberInfo => UserCodeMemberInfo.FromDelegate( this._validator );
}