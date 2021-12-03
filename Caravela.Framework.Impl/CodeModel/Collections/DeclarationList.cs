// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.References;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal abstract class DeclarationList<TDeclaration, TSource> : IReadOnlyList<TDeclaration>
        where TDeclaration : class, IDeclaration
        where TSource : ISdkRef<TDeclaration>
    {
        private volatile TDeclaration?[]? _targetItems;

        internal IDeclaration? ContainingDeclaration { get; }

        protected ImmutableArray<TSource> SourceItems { get; }

        public CompilationModel Compilation => (CompilationModel) this.ContainingDeclaration.AssertNotNull().Compilation;

        protected DeclarationList( IDeclaration? containingDeclaration, IEnumerable<TSource> sourceItems )
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
                if ( ((IRefImpl) item).Target != null )
                {
                    builder.Add( item );
                    canMoveToImmutable = false;
                }
            }

            this.SourceItems = canMoveToImmutable ? builder.MoveToImmutable() : builder.ToImmutable();
            this.ContainingDeclaration = containingDeclaration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeclarationList{TCodeElement,TSource}"/> class representing an empty list.
        /// </summary>
        protected DeclarationList()
        {
            this.SourceItems = ImmutableArray<TSource>.Empty;
        }

        public IEnumerator<TDeclaration> GetEnumerator()
        {
            for ( var i = 0; i < this.Count; i++ )
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public int Count => this.SourceItems.Length;

        public TDeclaration this[int index]
        {
            get
            {
                var targetItems = this._targetItems;

                if ( targetItems == null )
                {
                    _ = Interlocked.CompareExchange( ref this._targetItems, new TDeclaration?[this.SourceItems.Length], null );
                    targetItems = this._targetItems;
                }

                var targetItem = targetItems[index];

                if ( targetItem == null )
                {
                    targetItem = this.SourceItems[index].GetTarget( this.Compilation ).AssertNotNull();
                    _ = Interlocked.CompareExchange( ref targetItems[index], targetItem, null );
                }

                return targetItem;
            }
        }

        public override string ToString() => $"DeclarationList<{typeof( TDeclaration ).Name}> Count={this.Count}";
    }
}