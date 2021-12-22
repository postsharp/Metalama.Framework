// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Validation;
using System.Reflection;

namespace Metalama.Framework.Engine.Validation;

internal interface IValidatorDriverFactory
{
    ValidatorDriver<TContext> GetValidatorDriver<TContext>( MethodInfo validateMethod );
}

internal static class ValidatorDriverFactoryExtensions
{
    public static ValidatorDriver GetValidatorDriver( this IValidatorDriverFactory factory, MethodInfo method, ValidatorKind kind )
        => kind switch
        {
            ValidatorKind.Definition => factory.GetValidatorDriver<DeclarationValidationContext>( method ),
            ValidatorKind.Reference => factory.GetValidatorDriver<ReferenceValidationContext>( method ),
            _ => throw new AssertionFailedException()
        };
}