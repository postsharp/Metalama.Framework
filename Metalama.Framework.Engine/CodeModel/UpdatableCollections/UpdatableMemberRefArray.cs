// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal class UpdatableMemberRefArray<T>
    where T : class, IMemberOrNamedType
{
    // This is the only compilation in which the current object is mutable. It should not be mutable in other transformations.
    public CompilationModel ParentCompilation { get; }

    private ImmutableArray<MemberRef<T>> _array;

    public UpdatableMemberRefArray( ImmutableArray<MemberRef<T>> array, CompilationModel parentCompilation )
    {
        this._array = array;
        this.ParentCompilation = parentCompilation;
    }

    public ImmutableArray<MemberRef<T>> Array => this._array;

    public void Add( MemberRef<T> member )
    {
        this._array = this._array.Add( member );
    }

    public void Remove( MemberRef<T> member )
    {
        var index = this._array.IndexOf( member, MemberRefEqualityComparer<T>.Default );

        if ( index < 0 )
        {
            throw new AssertionFailedException();
        }

        this._array = this._array.RemoveAt( index );
    }
}