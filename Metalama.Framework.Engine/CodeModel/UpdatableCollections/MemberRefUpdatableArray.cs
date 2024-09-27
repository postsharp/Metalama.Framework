// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class MemberRefUpdatableArray<T>
    where T : class, ICompilationElement
{
    // This is the only compilation in which the current object is mutable. It should not be mutable in other transformations.
    public CompilationModel ParentCompilation { get; }

    public MemberRefUpdatableArray( ImmutableArray<IRef<T>> array, CompilationModel parentCompilation )
    {
        this.Array = array;
        this.ParentCompilation = parentCompilation;
    }

    public ImmutableArray<IRef<T>> Array { get; private set; }

    public void Add( IRef<T> member )
    {
        this.Array = this.Array.Add( member );
    }

    public void Remove( IRef<T> member )
    {
        var index = this.Array.IndexOf( member, RefEqualityComparer<T>.Default );

        if ( index < 0 )
        {
            throw new AssertionFailedException( $"The collection does not contain '{member}'." );
        }

        this.Array = this.Array.RemoveAt( index );
    }
}