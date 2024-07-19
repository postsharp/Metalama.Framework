// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class BuiltNamespace : BuiltNamedDeclaration, INamespace
{
    private readonly NamespaceBuilder _namespaceBuilder;

    public BuiltNamespace( CompilationModel compilation, NamespaceBuilder builder ) : base( compilation )
    {
        this._namespaceBuilder = builder;
    }

    public override DeclarationBuilder Builder => this._namespaceBuilder;

    protected override NamedDeclarationBuilder NamedDeclarationBuilder => this._namespaceBuilder;

    public string FullName => this._namespaceBuilder.FullName;

    public bool IsGlobalNamespace => this._namespaceBuilder.IsGlobalNamespace;

    public INamespace? ContainingNamespace => this._namespaceBuilder.ContainingNamespace;

    INamespace? INamespace.ParentNamespace => this.ContainingNamespace;

    public INamedTypeCollection Types
        => new NamedTypeCollection(
            this,
            this.Compilation.GetNamedTypeCollection( this._namespaceBuilder.ToRef().As<INamespaceOrNamedType>() ) );

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

    public INamespace GetDescendant( string ns ) => throw new System.NotImplementedException();

    public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = DerivedTypesOptions.Default )
        => throw new System.NotSupportedException();
}