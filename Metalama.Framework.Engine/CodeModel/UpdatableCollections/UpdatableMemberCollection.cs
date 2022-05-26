// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class UpdatableMemberCollection<T> : UpdatableDeclarationCollection<T>
    where T : class, IMemberOrNamedType
{
    protected UpdatableMemberCollection( CompilationModel compilation, INamespaceOrTypeSymbol declaringType ) : base( compilation )
    {
        this.DeclaringTypeOrNamespace = declaringType;
    }

    public INamespaceOrTypeSymbol DeclaringTypeOrNamespace { get; }

    public abstract void Add( MemberRef<T> member );

    public abstract void Remove( MemberRef<T> member );

    public abstract ImmutableArray<MemberRef<T>> OfName( string name );
}