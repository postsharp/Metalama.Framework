// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Metalama.Framework.Engine.Diagnostics
{
    /// <summary>
    /// Provides a method <see cref="ToArray"/> that converts a tuple into an array.
    /// </summary>
    [UsedImplicitly]
    internal sealed class ValueTupleAdapter
    {
        private static readonly ConcurrentDictionary<Type, Func<object, object[]>> _cache = new();

        public static object[] ToArray( object valueTuple ) => _cache.GetOrAdd( valueTuple.GetType(), CreateAdapter ).Invoke( valueTuple );

        private static Func<object, object[]> CreateAdapter( Type type )
        {
            var arity = type.GetGenericArguments().Length;
            var input = Expression.Parameter( typeof(object) );
            var convertedInput = Expression.Convert( input, type );
            var fields = new Expression[arity];

            for ( var i = 0; i < arity; i++ )
            {
                fields[i] = Expression.Convert( Expression.Field( convertedInput, $"Item{i + 1}" ), typeof(object) );
            }

            var array = Expression.NewArrayInit( typeof(object), fields );

            return Expression.Lambda<Func<object, object[]>>( array, input ).Compile();
        }
    }
}