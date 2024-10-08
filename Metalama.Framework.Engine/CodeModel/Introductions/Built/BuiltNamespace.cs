// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Built;

internal sealed class BuiltNamespace : BuiltNamedDeclaration, INamespace
{
    private readonly NamespaceBuilderData _namedDeclarationBuilder;

    public BuiltNamespace( NamespaceBuilderData builder, CompilationModel compilation ) : base( compilation, GenericContext.Empty )
    {
        this._namedDeclarationBuilder = builder;
    }

    public override DeclarationBuilderData BuilderData => this.NamedDeclarationBuilder;

    protected override NamedDeclarationBuilderData NamedDeclarationBuilder => this._namedDeclarationBuilder;

    [Memo]
    public string FullName => this.ContainingNamespace.IsGlobalNamespace ? this.Name : this.ContainingNamespace.FullName + "." + this.Name;

    public bool IsGlobalNamespace => false;

    [Memo]
    public INamespace ContainingNamespace => this.MapDeclaration( this.NamedDeclarationBuilder.ContainingDeclaration.As<INamespace>() );

    [Memo]
    private IFullRef<INamespace> Ref => this.RefFactory.FromBuilt<INamespace>( this );

    IRef<INamespace> INamespace.ToRef() => this.Ref;

    private protected override IFullRef<IDeclaration> ToFullDeclarationRef() => this.Ref;

    IRef<INamespaceOrNamedType> INamespaceOrNamedType.ToRef() => this.Ref;

    INamespace? INamespace.ParentNamespace => this.ContainingNamespace;

    public INamedTypeCollection Types
        => new NamedTypeCollection(
            this,
            this.Compilation.GetNamedTypeCollectionByParent( this._namedDeclarationBuilder.ToRef() ) );

    public INamespaceCollection Namespaces
        => new NamespaceCollection(
            this,
            this.Compilation.GetNamespaceCollection( this.NamedDeclarationBuilder.ToRef().As<INamespace>() ) );

    public bool IsPartial
    {
        get
        {
            var existingNamespace = this.Compilation.GlobalNamespace.GetDescendant( this.FullName );

            if ( existingNamespace != null )
            {
                return existingNamespace.IsPartial;
            }
            else
            {
                return false;
            }
        }
    }

    public INamespace GetDescendant( string ns ) => throw new NotImplementedException();

    public override bool CanBeInherited => false;

    public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = DerivedTypesOptions.Default )
        => throw new NotSupportedException();
}