﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.Introductions.BuilderData;

internal abstract class DeclarationBuilderData
{
    public abstract DeclarationKind DeclarationKind { get; }

    public IFullRef<IDeclaration> ContainingDeclaration { get; }

    public AspectLayerInstance ParentAdvice { get; }

    // Must be initialized from the final constructor when the reference to the current object is known.
    public ImmutableArray<AttributeBuilderData> Attributes { get; protected init; }

    public SyntaxTree? PrimarySyntaxTree { get; }

    protected DeclarationBuilderData( IDeclarationBuilderImpl builder, IFullRef<IDeclaration> containingDeclaration )
    {
        Invariant.Assert( builder.IsFrozen );
        Invariant.Assert( builder.DeclarationKind == DeclarationKind.Namespace || builder.PrimarySyntaxTree != null );

        this.ParentAdvice = builder.AspectLayerInstance;
        this.ContainingDeclaration = containingDeclaration;
        this.PrimarySyntaxTree = builder.PrimarySyntaxTree;
        this.IsDesignTimeObservable = builder.IsDesignTimeObservable;
    }

    public IFullRef<IDeclaration> ToRef() => this.ToDeclarationRef();

    protected abstract IFullRef<IDeclaration> ToDeclarationRef();

    public bool IsDesignTimeObservable { get; }

    /// <summary>
    /// Gets the declarations that are owned by the current <see cref="DeclarationBuilderData"/>, i.e. children that
    /// are not added to the <see cref="CompilationModel"/> by directly instantiated by us.
    /// </summary>
    public virtual IEnumerable<DeclarationBuilderData> GetOwnedDeclarations() => this.Attributes;

    public abstract IFullRef<INamedType>? DeclaringType { get; }

    // In the future, the InsertPosition could be influenced from the Builder, for instance to select a partial file.

    [Memo]
    public InsertPosition InsertPosition => this.GetInsertPosition();

    protected virtual InsertPosition GetInsertPosition() => throw new NotSupportedException();
}