// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.Builders.Data;

internal abstract class DeclarationBuilderData
{
    protected DeclarationBuilderData( IDeclarationBuilderImpl builder, IRef<IDeclaration> containingDeclaration )
    {
        this.ParentAdvice = builder.ParentAdvice;
        this.ContainingDeclaration = containingDeclaration;

        // ReSharper disable once VirtualMemberCallInConstructor
        this.Attributes = builder.Attributes.ToImmutable( this.ToDeclarationRef() );
    }

    public abstract IRef<IDeclaration> ToDeclarationRef();

    public abstract DeclarationKind DeclarationKind { get; }

    public IRef<IDeclaration> ContainingDeclaration { get; }

    public Advice ParentAdvice { get; }

    public ImmutableArray<AttributeBuilderData> Attributes { get; }

    public IRef<IDeclaration> ToRef() => this.ToDeclarationRef();

    public SerializableDeclarationId ToSerializableId() => throw new NotImplementedException();

}