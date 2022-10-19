// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel
{
    internal class Namespace : Declaration, INamespace
    {
        private readonly INamespaceSymbol _symbol;

        internal Namespace( INamespaceSymbol symbol, CompilationModel compilation, string fullName ) : base( compilation, symbol )
        {
            this._symbol = symbol;
            this.FullName = fullName;
        }

        public override DeclarationKind DeclarationKind => DeclarationKind.Namespace;

        public override ISymbol Symbol => this._symbol;

        public string Name => this._symbol.IsGlobalNamespace ? "" : this._symbol.Name;

        public string FullName { get; }

        public bool IsGlobalNamespace => this._symbol.IsGlobalNamespace;

        public override bool CanBeInherited => false;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => Enumerable.Empty<IDeclaration>();

        [Memo]
        public INamespace? ParentNamespace => this.IsGlobalNamespace ? null : this.Compilation.Factory.GetNamespace( this._symbol.ContainingNamespace );

        // TODO: TypeUpdatableCollection could be cached in the CompilationModel.
        public INamedTypeCollection Types
        {
            get
            {
                this.OnUnsupportedDependency( $"{nameof(INamespace)}.{nameof(this.Types)}" );

                return this.TypesCore;
            }
        }

        [Memo]
        private INamedTypeCollection TypesCore
            => new NamedTypeCollection(
                this,
                new TypeUpdatableCollection( this.Compilation, this._symbol ) );

        // TODO: AllNamespaceTypesUpdateableCollection could be cached in the CompilationModel.
        public INamedTypeCollection AllTypes
        {
            get
            {
                this.OnUnsupportedDependency( $"{nameof(INamespace)}.{nameof(this.AllTypes)}" );

                return this.AllTypesCore;
            }
        }

        [Memo]
        private INamedTypeCollection AllTypesCore
            => new NamedTypeCollection(
                this,
                new AllNamespaceTypesUpdateableCollection( this.Compilation, this._symbol ) );

        public INamespaceCollection Namespaces
        {
            get
            {
                this.OnUnsupportedDependency( $"{nameof(INamespace)}.{nameof(this.Namespaces)}" );

                return this.NamespacesCore;
            }
        }

        [Memo]
        private INamespaceCollection NamespacesCore
            => new NamespaceCollection(
                this,
                this._symbol.GetNamespaceMembers()
                    .Where( n => this.Compilation.PartialCompilation.ParentNamespaces.Contains( n ) )
                    .Select( n => new Ref<INamespace>( n, this.Compilation.RoslynCompilation ) )
                    .ToList() );

        public bool IsExternal => false;

        public override string ToString() => this.IsGlobalNamespace ? "<Global Namespace>" : this.FullName;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this.FullName;

        public override SyntaxTree? PrimarySyntaxTree => null;
    }
}