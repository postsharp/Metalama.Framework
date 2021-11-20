// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code.DeclarationBuilders;

namespace Caravela.Framework.Code
{
    public static class AttributeExtensions
    {
        public static AttributeConstruction ToAttributeConstruction( this IAttribute attribute )
            => AttributeConstruction.Create( attribute.Constructor, attribute.ConstructorArguments, attribute.NamedArguments );

        public static bool TryGetNamedArgument( this IAttribute attribute, string name, out TypedConstant value )
        {
            foreach ( var argument in attribute.NamedArguments )
            {
                if ( argument.Key == name )
                {
                    value = argument.Value;

                    return true;
                }
            }

            value = default;

            return false;
        }

        public static object? GetNamedArgumentValue( this IAttribute attribute, string name )
        {
            if ( attribute.TryGetNamedArgument( name, out var value ) )
            {
                return value.Value;
            }
            else
            {
                return null;
            }
        }
    }
}