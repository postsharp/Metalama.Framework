// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Utilities;
using System;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal class NamespaceBuilder : NamedDeclarationBuilder, INamespace
{
    public string FullName
        => this.ContainingNamespace != null
            ? $"{this.ContainingNamespace.FullName}.{this.Name}"
            : throw new AssertionFailedException("There should be a parent namespace.");

    public bool IsGlobalNamespace => false;

    public INamespace? ContainingNamespace { get; }

    INamespace? INamespace.ParentNamespace => this.ContainingNamespace;

    [Memo]
    public INamedTypeCollection Types => new EmptyNamedTypeCollection();

    [Memo]
    public INamespaceCollection Namespaces => new EmptyNamespaceCollection();

    public bool IsPartial => throw new NotImplementedException();

    public override IDeclaration? ContainingDeclaration => this.ContainingNamespace;

    public override DeclarationKind DeclarationKind => DeclarationKind.Namespace;

    public override bool CanBeInherited => false;

    public IntroduceNamespaceTransformation ToTransformation() => new( this.ParentAdvice, this );

    public NamespaceBuilder( Advice advice, INamespace containingNamespace, string name ) : base( advice, name )
    {
        this.ContainingNamespace = containingNamespace;
    }

    public INamespace? GetDescendant( string ns )
    {
        return null;
    }

    public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this.FullName;
}
