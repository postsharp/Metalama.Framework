// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class Namespace : Declaration, INamespace
    {
        private readonly INamespaceSymbol _symbol;

        internal Namespace( INamespaceSymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this._symbol = symbol;
        }

        private bool IsExternal => this._symbol.ContainingAssembly != this.Compilation.RoslynCompilation.Assembly;

        [Memo]
        public override IAssembly DeclaringAssembly
            => this.Compilation.Factory.GetAssembly(
                this._symbol.ContainingAssembly
                ?? throw new InvalidOperationException( "This namespace is a merged namespace for the whole compilation, so it has no declaring assembly." ) );

        public override DeclarationKind DeclarationKind => DeclarationKind.Namespace;

        public override ISymbol Symbol => this._symbol;

        public string Name => this._symbol.IsGlobalNamespace ? "" : this._symbol.Name;

        [Memo]
        public string FullName => this._symbol.GetFullName() ?? "";

        public bool IsGlobalNamespace => this._symbol.IsGlobalNamespace;

        public override bool CanBeInherited => false;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default ) => Enumerable.Empty<IDeclaration>();

        [Memo]
        public INamespace? ParentNamespace => this.IsGlobalNamespace ? null : this.Compilation.Factory.GetNamespace( this._symbol.ContainingNamespace );

        // TODO: TypeUpdatableCollection could be cached in the CompilationModel.
        public INamedTypeCollection Types
        {
            get
            {
                if ( !this.IsExternal )
                {
                    OnUnsupportedDependency( $"{nameof(INamespace)}.{nameof(this.Types)}" );
                }

                return this.TypesCore;
            }
        }

        [Memo]
        private INamedTypeCollection TypesCore => new NamedTypeCollection( this, new TypeUpdatableCollection( this.Compilation, this._symbol ) );

        // TODO: AllNamespaceTypesUpdateableCollection could be cached in the CompilationModel.

        public INamespaceCollection Namespaces
        {
            get
            {
                if ( !this.IsExternal )
                {
                    OnUnsupportedDependency( $"{nameof(INamespace)}.{nameof(this.Namespaces)}" );
                }

                return this.NamespacesCore;
            }
        }

        [Memo]
        private INamespaceCollection NamespacesCore
            => new NamespaceCollection(
                this,
                this._symbol.GetNamespaceMembers()
                    .Where( n => this.IsExternal || this.Compilation.PartialCompilation.ParentNamespaces.Contains( n ) )
                    .Select( n => new Ref<INamespace>( n, this.Compilation.CompilationContext ) )
                    .ToReadOnlyList() );

        public INamespace? GetDescendant( string ns )
        {
            var s = this._symbol.GetDescendant( ns );

            if ( s == null )
            {
                return null;
            }

            return this.Compilation.Factory.GetNamespace( s );
        }

        public bool IsPartial => !this.IsExternal && this.Compilation.IsPartial;

        public override string ToString() => this.IsGlobalNamespace ? "<Global Namespace>" : this.FullName;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this.FullName;

        public override SyntaxTree? PrimarySyntaxTree => null;
    }
}