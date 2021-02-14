﻿using System;
using System.Collections.Generic;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel.Symbolic;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{

    internal static class EnumerableExtensions
    {
        public static IReadOnlyList<T> ConcatNotNull<T>( this IReadOnlyList<T> a, T? b )
        {
            if ( b == null )
            {
                return a;
            }
            else if ( a == null || a.Count == 0 )
            {
                return new[] { b };
            }
            else
            {
                var l = new List<T>( a.Count + 1 );
                l.AddRange( a );
                l.Add( b );
                return l;
            }
        }

        public static IReadOnlyList<T> Concat<T>( this IReadOnlyList<T> a, IReadOnlyList<T>? b )
        {
            if ( b == null || b.Count == 0 )
            {
                return a;
            }
            else if ( a.Count == 0 )
            {
                return b;
            }
            else
            {
                var l = new List<T>( a.Count + b.Count );
                l.AddRange( a );
                l.AddRange( b );
                return l;
            }
        }
    }
}
