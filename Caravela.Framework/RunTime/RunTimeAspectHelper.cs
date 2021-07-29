// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable RedundantBlankLines, MissingBlankLines
#if NET5_0
using System.Threading;
using System.Threading.Tasks;
#endif

// ReSharper restore RedundantBlankLines, MissingBlankLines

namespace Caravela.Framework.RunTime
{
    public static class RunTimeAspectHelper
    {
        public static List<T> Buffer<T>( this IEnumerable<T> enumerable ) => enumerable as List<T> ?? new List<T>( enumerable );

        public static List<object> Buffer( this IEnumerable enumerable )
        {
            if ( enumerable is List<object> list )
            {
                return list;
            }
            else
            {
                list = new List<object>();

                foreach ( var item in enumerable )
                {
                    list.Add( item );
                }

                return list;
            }
        }

        public static List<T>.Enumerator Buffer<T>( this IEnumerator<T> enumerator )
        {
            if ( enumerator is List<T>.Enumerator listEnumerator )
            {
                return listEnumerator;
            }
            else
            {
                List<T> list = new();

                try
                {
                    while ( enumerator.MoveNext() )
                    {
                        list.Add( enumerator.Current );
                    }
                }
                finally
                {
                    enumerator.Dispose();
                }

                return list.GetEnumerator();
            }
        }

        public static List<object>.Enumerator Buffer( this IEnumerator enumerator )
        {
            if ( enumerator is List<object>.Enumerator listEnumerator )
            {
                return listEnumerator;
            }
            else
            {
                List<object> list = new();

                try
                {
                    while ( enumerator.MoveNext() )
                    {
                        list.Add( enumerator.Current );
                    }
                }
                finally
                {
                    (enumerator as IDisposable)?.Dispose();
                }

                return list.GetEnumerator();
            }
        }

#if NET5_0
        public static async ValueTask<AsyncEnumerableList<T>> BufferAsync<T>( this IAsyncEnumerable<T> enumerable, CancellationToken cancellationToken = default )
        {
            if ( enumerable is AsyncEnumerableList<T> asyncEnumerableList )
            {
                return asyncEnumerableList;
            }
            else
            {
                asyncEnumerableList = new AsyncEnumerableList<T>();

                await foreach ( var item in enumerable.WithCancellation( cancellationToken ) )
                {
                    asyncEnumerableList.Add( item );
                }

                return asyncEnumerableList;
            }
        }
        
        public static async ValueTask<AsyncEnumerableList<T>.AsyncEnumerator> BufferAsync<T>( this IAsyncEnumerator<T> enumerator, CancellationToken cancellationToken = default )
        {
            if ( enumerator is AsyncEnumerableList<T>.AsyncEnumerator typedEnumerator )
            {
                return typedEnumerator;
            }
            else
            {
                var list = new AsyncEnumerableList<T>();

                try
                {
                    while ( await enumerator.MoveNextAsync() )
                    {
                        list.Add( enumerator.Current );
                        
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
                finally
                {
                    await enumerator.DisposeAsync();
                }


                return list.GetAsyncEnumerator( cancellationToken );
            }
        }
#endif
    }
}