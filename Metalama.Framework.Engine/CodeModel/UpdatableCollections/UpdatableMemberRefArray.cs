﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal class UpdatableMemberRefArray<T>
    where T : class, IMemberOrNamedType
{
    // This is the only compilation in which the current object is mutable. It should not be mutable in other transformations.
    public CompilationModel ParentCompilation { get; }

    public UpdatableMemberRefArray( ImmutableArray<MemberRef<T>> array, CompilationModel parentCompilation )
    {
        this.Array = array;
        this.ParentCompilation = parentCompilation;
    }

    public ImmutableArray<MemberRef<T>> Array { get; private set; }

    public void Add( MemberRef<T> member )
    {
        this.Array = this.Array.Add( member );
    }

    public void Remove( MemberRef<T> member )
    {
        var index = this.Array.IndexOf( member, MemberRefEqualityComparer<T>.Default );

        if ( index < 0 )
        {
            throw new AssertionFailedException( $"The collection does not contain '{member}'." );
        }

        this.Array = this.Array.RemoveAt( index );
    }
}