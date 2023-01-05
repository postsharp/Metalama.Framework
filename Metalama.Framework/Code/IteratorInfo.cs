// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Information about an iterator method, returned by the <see cref="MethodExtensions.GetIteratorInfo"/> extension method of <see cref="IMethod"/>.
    /// </summary>    
    [CompileTime]
    public readonly struct IteratorInfo
    {
        private readonly IMethod? _method;

        /// <summary>
        /// Gets a value indicating whether the method is an iterator (i.e., has a <c>yield return</c> or <c>yield break</c> statement).
        /// This property evaluates to <c>false</c> for methods that return an enumerable type but do not use <c>yield</c>.
        /// </summary>
        public bool IsIterator { get; }

        /// <summary>
        /// Gets the type of items being enumerated (the <c>int</c> in <c>IEnumerable&lt;int&gt;</c>).
        /// </summary>
        public IType ItemType
        {
            get
            {
                if ( this._method == null )
                {
                    throw new InvalidOperationException( $"Cannot get the {nameof( this.ItemType )} property because the method is not available." );
                }

                if ( this._method.ReturnType is INamedType { TypeArguments.Count: > 0 } namedType )
                {
                    return namedType.TypeArguments[0];
                }
                else
                {
                    return TypeFactory.GetType( SpecialType.Object );
                }
            }
        }

        /// <summary>
        /// Gets the kind of enumerable (<see cref="Code.EnumerableKind.IEnumerable"/>, <see cref="Code.EnumerableKind.IEnumerator"/>,
        /// <see cref="Code.EnumerableKind.IAsyncEnumerable"/>, ...), regardless of whether the method is a yield-base iterator (see <see cref="IsIterator"/>).
        /// </summary>
        public EnumerableKind EnumerableKind { get; }

        internal IteratorInfo( bool isIterator, EnumerableKind enumerableKind, IMethod? method )
        {
            this._method = method;
            this.EnumerableKind = enumerableKind;
            this.IsIterator = isIterator;
        }
    }
}