// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Extension methods for the <see cref="IAttribute"/> interface.
    /// </summary>
    [CompileTime]
    public static class AttributeExtensions
    {
        /// <summary>
        /// Converts an <see cref="IAttribute"/> to an <see cref="AttributeConstruction"/> object.
        /// </summary>
        public static AttributeConstruction ToAttributeConstruction( this IAttribute attribute )
            => AttributeConstruction.Create( attribute.Constructor, attribute.ConstructorArguments, attribute.NamedArguments );

        /// <summary>
        /// Tries to get a named argument (i.e. the value assigned to a field or property).
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns><c>true</c> if the attribute defines this named argument, otherwise <c>false</c>.</returns>
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

        internal static object? GetNamedArgumentValue( this IAttribute attribute, string name )
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