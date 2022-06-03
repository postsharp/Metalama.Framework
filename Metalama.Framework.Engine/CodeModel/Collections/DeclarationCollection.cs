// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal abstract class DeclarationCollection<TDeclaration, TRef> : IReadOnlyCollection<TDeclaration>
        where TDeclaration : class, IDeclaration
        where TRef : IRefImpl<TDeclaration>
    {
        internal IDeclaration? ContainingDeclaration { get; }

        protected IReadOnlyList<TRef> Source { get; }

        public CompilationModel Compilation => (CompilationModel) this.ContainingDeclaration.AssertNotNull().Compilation;

        protected DeclarationCollection( IDeclaration containingDeclaration, IReadOnlyList<TRef> source )
        {
            this.Source = source;

            this.ContainingDeclaration = containingDeclaration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeclarationCollection{TDeclaration, TRef}"/> class representing an empty list.
        /// </summary>
        protected DeclarationCollection()
        {
            this.Source = ImmutableArray<TRef>.Empty;
        }

        public IEnumerator<TDeclaration> GetEnumerator()
        {
            foreach ( var sourceItem in this.Source )
            {
                yield return this.GetItem( sourceItem );
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public int Count => this.Source.Count;

        protected TDeclaration GetItem( in TRef reference ) => reference.GetTarget( this.Compilation );

        protected IEnumerable<TDeclaration> GetItems( IEnumerable<Ref<TDeclaration>> references ) => references.Select( x => x.GetTarget( this.Compilation ) );

        public override string ToString()
        {
            if ( this.Source is ILazy { IsComplete: true } )
            {
                return $"{this.GetType().Name} Count={this.Count}";
            }
            else
            {
                return $"{this.GetType().Name} (unresolved)";
            }
        }
    }
}