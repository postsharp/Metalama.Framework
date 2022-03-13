// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel
{
    internal class Namespace : Declaration, INamespace
    {
        private readonly INamespaceSymbol _symbol;

        internal Namespace( INamespaceSymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this._symbol = symbol;
        }

        public override DeclarationKind DeclarationKind => DeclarationKind.Namespace;

        public override ISymbol Symbol => this._symbol;

        public string Name => this._symbol.IsGlobalNamespace ? "" : this._symbol.Name;

        public string FullName => this._symbol.IsGlobalNamespace ? "" : this._symbol.ToDisplayString();

        public bool IsGlobalNamespace => this._symbol.IsGlobalNamespace;

        public override bool CanBeInherited => false;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => Enumerable.Empty<IDeclaration>();

        [Memo]
        public INamespace? ParentNamespace => this.IsGlobalNamespace ? null : this.Compilation.Factory.GetNamespace( this._symbol.ContainingNamespace );

        [Memo]
        public INamedTypeList Types
            => new NamedTypeList(
                this,
                this._symbol.GetTypeMembers()
                    .Where( t => this.Compilation.ContainsType( t ) )
                    .Select( n => new MemberRef<INamedType>( n, this.Compilation.RoslynCompilation ) ) );

        [Memo]
        public INamedTypeList AllTypes
            => new NamedTypeList(
                this,
                this._symbol.SelectManyRecursive( ns => ns.GetNamespaceMembers(), includeThis: true )
                    .SelectMany( ns => ns.GetTypeMembers() )
                    .Where( t => this.Compilation.ContainsType( t ) )
                    .Select( n => new MemberRef<INamedType>( n, this.Compilation.RoslynCompilation ) ) );

        public INamespaceList Namespaces
            => new NamespaceList(
                this,
                this._symbol.GetNamespaceMembers()
                    .Where( n => this.Compilation.PartialCompilation.ParentNamespaces.Contains( n ) )
                    .Select( n => new Ref<INamespace>( n, this.Compilation.RoslynCompilation ) ) );

        public bool IsAncestorOf( INamespace ns )
        {
            for ( var i = ns.ParentNamespace; i != null; i = i.ParentNamespace )
            {
                if ( i == this )
                {
                    return true;
                }
            }

            return false;
    }

        public bool IsDescendantOf( INamespace ns ) => ns.IsAncestorOf( this );
        

        public override string ToString() => this.IsGlobalNamespace ? "<Global Namespace>" : this.FullName;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            // Always write in full.
            return this._symbol.ToDisplayString();
        }
    }
}