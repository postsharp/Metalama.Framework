// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Builders.Data;
using Metalama.Framework.Engine.CodeModel.Collections;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Builders.Built;

internal sealed class BuiltNamespace : BuiltNamedDeclaration, INamespace
{
    private readonly NamespaceBuilderData _namespaceBuilder;

    public BuiltNamespace( NamespaceBuilderData builder, CompilationModel compilation ) : base( compilation, GenericContext.Empty )
    {
        this._namespaceBuilder = builder;
    }

    public override DeclarationBuilderData BuilderData => this._namespaceBuilder;

    protected override NamespaceBuilderData NamedDeclarationBuilder => this._namespaceBuilder;

    public string FullName => this._namespaceBuilder.FullName;

    public bool IsGlobalNamespace => false;

    public INamespace? ContainingNamespace => this._namespaceBuilder.ContainingNamespace;

    private IRef<INamespace> Ref => this._namespaceBuilder.Ref;

    IRef<INamespace> INamespace.ToRef() => this.Ref;

    private protected override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

    IRef<INamespaceOrNamedType> INamespaceOrNamedType.ToRef() => this.Ref;

    INamespace? INamespace.ParentNamespace => this.ContainingNamespace;

    public INamedTypeCollection Types
        => new NamedTypeCollection(
            this,
            this.Compilation.GetNamedTypeCollectionByParent( this._namespaceBuilder.ToRef() ) );

    public INamespaceCollection Namespaces
        => new NamespaceCollection(
            this,
            this.Compilation.GetNamespaceCollection( this._namespaceBuilder.ToRef().As<INamespace>() ) );

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

    public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = DerivedTypesOptions.Default )
        => throw new NotSupportedException();
}