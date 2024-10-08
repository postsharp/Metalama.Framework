// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Introductions.Collections;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Builders;

internal sealed class NamespaceBuilder : NamedDeclarationBuilder, INamespace
{
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

    public IntroduceNamespaceTransformation ToTransformation()
    {
        this.Freeze();

        return new IntroduceNamespaceTransformation( this.ParentAdvice, this.Immutable );
    }

    public NamespaceBuilder( AdviceInfo advice, INamespace containingNamespace, string name ) : base( advice, name )
    {
        this.ContainingNamespace = containingNamespace;
    }

    public override SyntaxTree? PrimarySyntaxTree => null;

    public INamespace? GetDescendant( string ns )
    {
        // TODO: Implement this.
        return null;
    }

    IRef<INamespace> INamespace.ToRef() => this.Immutable.ToRef();

    IRef<INamespaceOrNamedType> INamespaceOrNamedType.ToRef() => this.Immutable.ToRef();

    [Memo]
    public NamespaceBuilderData Immutable => new( this.AssertFrozen(), this.ContainingDeclaration.AssertNotNull().ToFullRef() );
}