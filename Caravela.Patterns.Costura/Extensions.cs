using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Patterns.Costura
{
    public static class Extensions
    {
        public static bool GetSafeBool(this ImmutableArray<KeyValuePair<string, TypedConstant>> collection, string name, bool defaultValue)
        {
            var found = collection.SingleOrDefault(kvp => kvp.Key == name);

            if (found.Key == null)
                return defaultValue;

            return (bool)found.Value.Value;
        }
        public static string[] GetSafeStringArray(this ImmutableArray<KeyValuePair<string, TypedConstant>> collection, string name)
        {
            var found = collection.SingleOrDefault(kvp => kvp.Key == name);

            if (found.Key == null)
                return new string[0];

            return found.Value.Values.Select(v => (string)v.Value).ToArray() ?? new string[0];
        }
    }
}