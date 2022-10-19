// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;
using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal class NamedTypeCollection : MemberOrNamedTypeCollection<INamedType>, INamedTypeCollection
    {
        public static NamedTypeCollection Empty { get; } = new();

        private NamedTypeCollection() { }

        public NamedTypeCollection( NamedType declaringType, UpdatableMemberCollection<INamedType> sourceItems ) :
            base( declaringType, sourceItems ) { }

        public NamedTypeCollection( ICompilation declaringType, UpdatableMemberCollection<INamedType> sourceItems ) :
            base( declaringType, sourceItems ) { }

        public NamedTypeCollection( INamespace declaringType, UpdatableMemberCollection<INamedType> sourceItems ) :
            base( declaringType, sourceItems ) { }

        IReadOnlyList<INamedType> INamedTypeCollection.DerivedFrom( Type type ) => throw new NotImplementedException();

        IReadOnlyList<INamedType> INamedTypeCollection.DerivedFrom( INamedType type ) => throw new NotImplementedException();
    }

    internal class ExternalTypesCollection : INamedTypeCollection
    {
        private readonly INamespaceSymbol _symbol;
        private readonly CompilationModel _compilation;
        private List<INamedTypeSymbol>? _types;

        public ExternalTypesCollection( INamespaceSymbol symbol, CompilationModel compilation )
        {
            this._symbol = symbol;
            this._compilation = compilation;
        }

        private List<INamedTypeSymbol> GetContent()
        {
            if ( this._types == null )
            {
                this._types = this._symbol.GetTypeMembers().Where( t => !this.IsHidden( t ) ).ToList();
            }

            return this._types;
        }

        private bool IsHidden( INamedTypeSymbol type ) => type.ContainingAssembly == this._compilation.RoslynCompilation.Assembly;

        public IEnumerable<INamedType> OfName( string name )
            => this._symbol.GetTypeMembers( name ).Where( t => !this.IsHidden( t ) ).Select( x => this._compilation.Factory.GetNamedType( x ) );

        public IEnumerator<INamedType> GetEnumerator()
        {
            foreach ( var type in this.GetContent() )
            {
                yield return this._compilation.Factory.GetNamedType( type );
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public int Count => this.GetContent().Count;

        public IReadOnlyList<INamedType> DerivedFrom( Type type ) => throw new NotImplementedException();

        public IReadOnlyList<INamedType> DerivedFrom( INamedType type ) => throw new NotImplementedException();
    }

    internal class ExternalNamespaceCollection : INamespaceCollection
    {
        private readonly INamespaceSymbol _symbol;
        private readonly CompilationModel _compilation;
        private List<INamespaceSymbol>? _children;

        public ExternalNamespaceCollection( INamespaceSymbol symbol, CompilationModel compilation )
        {
            this._symbol = symbol;
            this._compilation = compilation;
        }

        private List<INamespaceSymbol> GetContent()
        {
            if ( this._children == null )
            {
                this._children = this._symbol.GetNamespaceMembers().ToList();
            }

            return this._children;
        }

        public INamespace? OfName( string name )
        {
            var symbol = this.GetContent().FirstOrDefault( ns => ns.Name == name );

            if ( symbol == null )
            {
                return null;
            }
            else
            {
                return this._compilation.Factory.GetNamespace( name );
            }
        }

        public IEnumerator<INamespace> GetEnumerator()
        {
            foreach ( var ns in this.GetContent() )
            {
                yield return this._compilation.Factory.GetNamespace( ns );
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public int Count => this.GetContent().Count;
    }
}