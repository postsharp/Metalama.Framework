using System;
using System.Collections.Generic;

namespace Caravela.Framework.LinqPad
{
    public class PropertyComparer : IComparer<(string Name, Type Type)>
    {
        private PropertyComparer() { }

        public static readonly PropertyComparer Instance = new();

        private static int GetPropertyNamePriority( string s )
            => s switch
            {
                "Index" => 0,
                "Position" => 1,
                "Name" => 2,
                _ => 10
            };

        private static int GetPropertyTypePriority( Type t ) => t.GetInterface( "IEnumerable`1" ) != null ? 1 : 0;

        public int Compare( (string Name, Type Type) x, (string Name, Type Type) y )
        {
            var priorityComparison = GetPropertyNamePriority( x.Name ).CompareTo( GetPropertyNamePriority( y.Name ) );

            if ( priorityComparison != 0 )
            {
                return priorityComparison;
            }

            var typeComparison = GetPropertyTypePriority( x.Type ).CompareTo( GetPropertyTypePriority( y.Type ) );

            if ( typeComparison != 0 )
            {
                return typeComparison;
            }

            return StringComparer.Ordinal.Compare( x.Name, y.Name );
        }
    }
}