// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.CodeModel.Builders.Data;

internal abstract class MemberOrNamedTypeBuilderData : NamedDeclarationBuilderData
{
    protected MemberOrNamedTypeBuilderData( IMemberOrNamedTypeBuilderImpl builder, IRef<IDeclaration> containingDeclaration ) : base(
        builder,
        containingDeclaration )
    {
        this.Accessibility = builder.Accessibility;
        this.IsSealed = builder.IsSealed;
        this.IsNew = builder.IsNew;
        this.HasNewKeyword = builder.HasNewKeyword.AssertNotNull();
        this.IsAbstract = builder.IsAbstract;
        this.IsStatic = builder.IsStatic;
    }

    public Accessibility Accessibility { get; }

    public bool IsSealed { get; }

    public bool IsNew { get; }

    public bool HasNewKeyword { get; }

    public bool IsAbstract { get; }

    public bool IsStatic { get; }
}