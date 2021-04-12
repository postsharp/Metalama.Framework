// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// Provides a method <see cref="ToArray"/> that converts a tuple into an array.
    /// </summary>
    internal sealed class ValueTupleAdapter
    {
        private static readonly ConcurrentDictionary<Type, Func<object, object[]>> _cache = new ConcurrentDictionary<Type, Func<object, object[]>>();

        public static object[] ToArray( object valueTuple )
            => _cache.GetOrAdd( valueTuple.GetType(), CreateAdapter ).Invoke( valueTuple );

        private static Func<object, object[]> CreateAdapter( Type type )
        {
            var arity = type.GetGenericArguments().Length;
            var input = Expression.Parameter( typeof( object ) );
            var convertedInput = Expression.Convert( input, type );
            var fields = new Expression[arity];

            for ( var i = 0; i < arity; i++ )
            {
                fields[i] = Expression.Convert( Expression.Field( convertedInput, $"Item{i + 1}" ), typeof( object ) );
            }

            var array = Expression.NewArrayInit( typeof( object ), fields );
            return Expression.Lambda<Func<object, object[]>>( array, input ).Compile();
        }
    }
}