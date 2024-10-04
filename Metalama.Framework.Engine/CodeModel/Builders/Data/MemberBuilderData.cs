// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.CodeModel.Builders.Data;

internal abstract class MemberBuilderData : MemberOrNamedTypeBuilderData
{
    protected MemberBuilderData( IMemberBuilderImpl builder, IRef<IDeclaration> containingDeclaration ) : base( builder, containingDeclaration )
    {
        this.IsVirtual = builder.IsVirtual;
        this.IsAsync = builder.IsAsync;
        this.IsOverride = builder.IsOverride;
    }

    public bool IsVirtual { get; }

    public bool IsAsync { get; }

    public bool IsOverride { get; }
}