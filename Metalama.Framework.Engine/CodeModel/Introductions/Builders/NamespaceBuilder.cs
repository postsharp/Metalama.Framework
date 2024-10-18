// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Introductions.BuilderData;
using Metalama.Framework.Engine.CodeModel.Introductions.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Builders;

internal sealed class NamespaceBuilder : NamedDeclarationBuilder, INamespace
{
    public IntroducedRef<INamespace> Ref { get; }

    public NamespaceBuilder( AspectLayerInstance aspectLayerInstance, INamespace containingNamespace, string name ) : base( aspectLayerInstance, name )
    {
        this.ContainingNamespace = containingNamespace;
        this.Ref = new IntroducedRef<INamespace>( this.Compilation.RefFactory );
    }

    public string FullName
        => !this.ContainingNamespace.AssertNotNull().IsGlobalNamespace
            ? $"{this.ContainingNamespace.FullName}.{this.Name}"
            : this.Name;

    public bool IsGlobalNamespace => false;

    public INamespace? ContainingNamespace { get; }

    INamespace? INamespace.ParentNamespace => this.ContainingNamespace;

    [Memo]
    public INamedTypeCollection Types => new EmptyNamedTypeCollection();

    [Memo]
    public INamespaceCollection Namespaces => new EmptyNamespaceCollection();

    public bool IsPartial => false;

    public override bool IsDesignTimeObservable => true;

    public override IDeclaration? ContainingDeclaration => this.ContainingNamespace;

    public override DeclarationKind DeclarationKind => DeclarationKind.Namespace;

    public override bool CanBeInherited => false;

    public override SyntaxTree? PrimarySyntaxTree => null;

    public INamespace? GetDescendant( string ns )
    {
        // TODO: Implement this.
        return null;
    }

    IRef<INamespace> INamespace.ToRef() => this.Ref;

    protected override IFullRef<IDeclaration> ToFullDeclarationRef() => this.Ref;

    IRef<INamespaceOrNamedType> INamespaceOrNamedType.ToRef() => this.Ref;

    protected override void EnsureReferenceInitialized()
    {
        this.Ref.BuilderData = new NamespaceBuilderData( this, this.ContainingDeclaration.AssertNotNull().ToFullRef() );
    }

    public NamespaceBuilderData BuilderData => (NamespaceBuilderData) this.Ref.BuilderData;
}