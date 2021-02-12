// unset

using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    public static class NamedArgumentsExtensions
    {
        public static bool TryGetByName( this IReadOnlyList<KeyValuePair<string, object?>> arguments, string name, out object? value )
        {
            foreach ( var arg in arguments )
            {
                if ( arg.Key == name )
                {
                    value = arg.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        public static object? GetByName( this IReadOnlyList<KeyValuePair<string, object?>> arguments, string name )
        {
            if ( arguments.TryGetByName( name, out var value ) )
            {
                return value;
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }
    }
}