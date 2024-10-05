// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Data;

internal abstract class PropertyOrIndexerBuilderData : MemberBuilderData
{
    protected PropertyOrIndexerBuilderData( PropertyOrIndexerBuilder builder, IRef<INamedType> containingDeclaration ) : base( builder, containingDeclaration )
    {

        this.Type = builder.Type.ToRef();
        this.HasInitOnlySetter = builder.HasInitOnlySetter;
        this.RefKind = builder.RefKind;
        this.Writeability = builder.Writeability;

    }

    public IRef<IType> Type { get; }

    public bool HasInitOnlySetter { get; }

    public RefKind RefKind { get; set; }

    // Accessors are abstract because we can't initialize them from the constructor because we don't have a reference to ourselves yet.
    public abstract MethodBuilderData? GetMethod { get; }

    public abstract MethodBuilderData? SetMethod { get; }

    public Writeability Writeability { get; }
}