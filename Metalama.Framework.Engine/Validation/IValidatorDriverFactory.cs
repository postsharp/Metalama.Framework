// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Validation;

namespace Metalama.Framework.Engine.Validation;

internal interface IValidatorDriverFactory
{
    ValidatorDriver<TContext> GetValidatorDriver<TContext>( string name );
}

internal static class ValidatorDriverFactoryExtensions
{
    public static ValidatorDriver GetValidatorDriver( this IValidatorDriverFactory factory, string name, ValidatorKind kind )
        => kind switch
        {
            ValidatorKind.Definition => factory.GetValidatorDriver<DeclarationValidationContext>( name ),
            ValidatorKind.Reference => factory.GetValidatorDriver<ReferenceValidationContext>( name ),
            _ => throw new AssertionFailedException()
        };
}