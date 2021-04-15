// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal abstract class CodeElementList<TCodeElement, TSource> : IReadOnlyList<TCodeElement>
        where TCodeElement : class, ICodeElement
        where TSource : ICodeElementLink<TCodeElement>
    {
        private readonly CompilationModel? _compilation;
        private volatile TCodeElement?[]? _targetItems;

        protected ImmutableArray<TSource> SourceItems { get; }

        protected CodeElementList( IEnumerable<TSource> sourceItems, CompilationModel compilation )
        {
            ImmutableArray<TSource>.Builder? builder;
            bool canMoveToImmutable;

            if ( sourceItems is IReadOnlyCollection<TSource> collection )
            {
                builder = ImmutableArray.CreateBuilder<TSource>( collection.Count );
                canMoveToImmutable = true;
            }
            else
            {
                builder = ImmutableArray.CreateBuilder<TSource>();
                canMoveToImmutable = false;
            }

            foreach ( var item in sourceItems )
            {
                if ( item.Target != null )
                {
                    builder.Add( item );
                    canMoveToImmutable = false;
                }
            }

            this.SourceItems = canMoveToImmutable ? builder.MoveToImmutable() : builder.ToImmutable();
            this._compilation = compilation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeElementList{TCodeElement, TSource}"/> class representing an empty list.
        /// </summary>
        protected CodeElementList()
        {
            this.SourceItems = ImmutableArray<TSource>.Empty;
        }

        public IEnumerator<TCodeElement> GetEnumerator()
        {
            for ( var i = 0; i < this.Count; i++ )
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public int Count => this.SourceItems.Length;

        public TCodeElement this[ int index ]
        {
            get
            {
                var targetItems = this._targetItems;

                if ( targetItems == null )
                {
                    _ = Interlocked.CompareExchange( ref this._targetItems, new TCodeElement?[this.SourceItems.Length], null );
                    targetItems = this._targetItems;
                }

                var targetItem = targetItems[index];

                if ( targetItem == null )
                {
                    targetItem = this.SourceItems[index].GetForCompilation( this._compilation! );
                    _ = Interlocked.CompareExchange( ref targetItems[index], targetItem, null );
                }

                return targetItem;
            }
        }
    }
}