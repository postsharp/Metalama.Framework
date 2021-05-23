using Caravela.Framework.Code;
using System;
using System.Linq;

namespace Caravela.Framework.Project
{
    public static class QueryableExtensions
    {
        // 'DerivedFrom' is the only predicate that we want to process semantically because we can optimize it.
        // For other predicates, there is no benefit to optimize them with a special extension method so we can use
        // the rest of the code model based on IEnumerable instead of IQueryable.
        
        public static IQueryable<INamedType> DerivedFrom( this IQueryable<INamedType> types, Type type ) => throw new NotImplementedException();
    }
}