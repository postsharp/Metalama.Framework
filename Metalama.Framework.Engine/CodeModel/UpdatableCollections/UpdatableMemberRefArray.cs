// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class UpdatableMemberRefArray<T>
    where T : class, IRef
{
    private readonly IEqualityComparer<T> _comparer;

    // This is the only compilation in which the current object is mutable. It should not be mutable in other transformations.
    public CompilationModel ParentCompilation { get; }

    public UpdatableMemberRefArray( ImmutableArray<T> array, CompilationModel parentCompilation, IEqualityComparer<T> comparer )
    {
        this.Array = array;
        this.ParentCompilation = parentCompilation;
        this._comparer = comparer;
    }

    public ImmutableArray<T> Array { get; private set; }

    public void Add( T member )
    {
        this.Array = this.Array.Add( member );
    }

    public void Remove( T member )
    {
        var index = this.Array.IndexOf( member, this._comparer );

        if ( index < 0 )
        {
            throw new AssertionFailedException( $"The collection does not contain '{member}'." );
        }

        this.Array = this.Array.RemoveAt( index );
    }
}