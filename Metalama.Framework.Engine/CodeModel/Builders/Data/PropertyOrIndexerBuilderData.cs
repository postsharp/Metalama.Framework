// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.CodeModel.Builders.Data;

internal abstract class PropertyOrIndexerBuilderData : MemberOrNamedTypeBuilderData
{
    protected PropertyOrIndexerBuilderData( PropertyOrIndexerBuilder builder, IRef<INamedType> containingDeclaration ) : base( builder, containingDeclaration )
    {
        var me = this.ToDeclarationRef();

        this.Type = builder.Type.ToRef();
        this.HasInitOnlySetter = builder.HasInitOnlySetter;
        this.RefKind = builder.RefKind;
        this.Writeability = builder.Writeability;

        if ( builder.GetMethod != null )
        {
            this.GetMethod = new MethodBuilderData( builder.GetMethod, me );
        }

        if ( builder.SetMethod != null )
        {
            this.SetMethod = new MethodBuilderData( builder.SetMethod, me );
        }
    }

    public IRef<IType> Type { get; }

    public bool HasInitOnlySetter { get; }

    public RefKind RefKind { get; set; }

    public MethodBuilderData? GetMethod { get; }

    public MethodBuilderData? SetMethod { get; }

    public Writeability Writeability { get;  }
}