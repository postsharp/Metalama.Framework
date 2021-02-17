using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Code
{
    public static class CodeElementExtensions
    {
        public static IEnumerable<T> OfName<T>( this IEnumerable<T> members, string name )
            where T : IMember
            => members.Where( m => m.Name == name );
    }
}