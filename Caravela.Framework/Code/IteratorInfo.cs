// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Code
{
    public readonly struct IteratorInfo
    {
        private readonly object? _method;
        private readonly Func<object, IType>? _getItemType;

        public bool IsIterator => this.IteratorKind != IteratorKind.None;

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

        public IteratorKind IteratorKind { get; }

        public bool IsAsync => this.IteratorKind is IteratorKind.IAsyncEnumerable or IteratorKind.IAsyncEnumerator;

        internal IteratorInfo( IteratorKind iteratorKind, object? method, Func<object, IType> getItemType )
        {
            this._method = method;
            this._getItemType = getItemType;
            this.IteratorKind = iteratorKind;
        }

        internal IteratorInfo( IteratorKind iteratorKind )
        {
            this.IteratorKind = iteratorKind;
            this._method = null;
            this._getItemType = null;
        }
    }
}