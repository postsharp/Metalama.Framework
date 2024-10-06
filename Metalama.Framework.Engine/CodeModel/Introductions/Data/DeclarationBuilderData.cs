// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Data;

internal abstract class DeclarationBuilderData
{
    public abstract DeclarationKind DeclarationKind { get; }

    public IRef<IDeclaration> ContainingDeclaration { get; }

    [Obsolete( "We want to get rid of this." )]
    public Advice ParentAdvice { get; }

    public ImmutableArray<AttributeBuilderData> Attributes { get; }

    public SyntaxTree? PrimarySyntaxTree { get; }

    protected DeclarationBuilderData( IDeclarationBuilderImpl builder, IRef<IDeclaration> containingDeclaration )
    {
        Invariant.Assert( builder.IsFrozen );

        this.ParentAdvice = builder.ParentAdvice;
        this.ContainingDeclaration = containingDeclaration;
        this.PrimarySyntaxTree = builder.PrimarySyntaxTree;
        this.IsDesignTimeObservable = builder.IsDesignTimeObservable;

        // ReSharper disable once VirtualMemberCallInConstructor
        this.Attributes = builder.Attributes.ToImmutable( this.ToDeclarationRef() );
    }

    public IRef<IDeclaration> ToRef() => this.ToDeclarationRef();

    protected abstract IRef<IDeclaration> ToDeclarationRef();

    public SerializableDeclarationId ToSerializableId() => throw new NotImplementedException();

    public bool IsDesignTimeObservable { get; }

    /// <summary>
    /// Gets the declarations that are owned by the current <see cref="DeclarationBuilderData"/>, i.e. children that
    /// are not added to the <see cref="CompilationModel"/> by directly instantiated by us.
    /// </summary>
    public virtual IEnumerable<DeclarationBuilderData> GetOwnedDeclarations() => this.Attributes;
}