// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Information about iterator method, returned by the <see cref="MethodExtensions.GetIteratorInfo"/> extension method of <see cref="IMethod"/>.
    /// </summary>
    public readonly struct IteratorInfo
    {
        private readonly object? _method;
        private readonly Func<object, IType>? _getItemType;

        /// <summary>
        /// Gets a value indicating whether the method is an iterator (i.e., has a <c>yield return</c> or <c>yield break</c> statement).
        /// </summary>
        public bool IsIterator => this.IteratorKind != IteratorKind.None;

        /// <summary>
        /// Gets the type of items being enumerated (the <c>int</c> in <c>IEnumerable&lt;int&gt;</c>).
        /// </summary>
        public IType ItemType
        {
            get
            {
                if ( this._method == null )
                {
                    throw new InvalidOperationException( $"Cannot get the {nameof(this.ItemType)} property because the method is not available." );
                }

                return this._getItemType!( this._method );
            }
        }

        /// <summary>
        /// Gets the kind of iterator (<see cref="Code.IteratorKind.IEnumerable"/>, <see cref="Code.IteratorKind.IEnumerator"/>,
        /// <see cref="Code.IteratorKind.IAsyncEnumerable"/>, ...).
        /// </summary>
        public IteratorKind IteratorKind { get; }

        /// <summary>
        /// Gets a value indicating whether the iterator is an async iterator, i.e. its <see cref="IteratorKind"/> is
        /// <see cref="Code.IteratorKind.IAsyncEnumerable"/> or <see cref="Code.IteratorKind.IAsyncEnumerator"/>.
        /// </summary>
        public bool IsAsync => this.IteratorKind is IteratorKind.IAsyncEnumerable or IteratorKind.IAsyncEnumerator;

        internal IteratorInfo( IteratorKind iteratorKind, object? method, Func<object, IType> getItemType )
        {
            this._method = method;
            this._getItemType = getItemType;
            this.IteratorKind = iteratorKind;
        }

    }
}