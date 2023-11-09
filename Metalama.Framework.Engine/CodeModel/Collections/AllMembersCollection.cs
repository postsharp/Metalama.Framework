// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Services;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal abstract class AllMembersCollection<T> : IMemberCollection<T>
    where T : class, IMember
{
    private volatile HashSet<T>? _members;

    protected AllMembersCollection( NamedType declaringType )
    {
        this.DeclaringType = declaringType;
    }

    INamedType IMemberCollection<T>.DeclaringType => this.DeclaringType;

    protected CompilationContext CompilationContext => this.DeclaringType.Compilation.CompilationContext;

    private NamedType DeclaringType { get; }

    public IEnumerable<T> OfName( string name ) => this.GetItemsCore( name );

    public IEnumerator<T> GetEnumerator() => this.GetItems().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    protected abstract IMemberCollection<T> GetMembers( INamedType namedType );

    public int Count => this.GetItems().Count;

    protected abstract IEqualityComparer<T> Comparer { get; }

    private HashSet<T> GetItemsCore( string? name )
    {
        // We don't assign the field directly so we don't get into concurrent updates of the collection.
        var members = new HashSet<T>( this.Comparer );

        for ( var t = (INamedType) this.DeclaringType; t != null; t = t.BaseType )
        {
            var includePrivate = ReferenceEquals( t, this.DeclaringType );
            var declaredMembers = name == null ? this.GetMembers( t ) : this.GetMembers( t ).OfName( name );

            foreach ( var member in declaredMembers )
            {
                if ( includePrivate || member.Accessibility != Accessibility.Private )
                {
                    members.Add( member );
                }
            }
        }

        return members;
    }

    private HashSet<T> GetItems()
    {
        if ( this._members == null )
        {
            Interlocked.CompareExchange( ref this._members, this.GetItemsCore( null ), null );
        }

        return this._members;
    }
}