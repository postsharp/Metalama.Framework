// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Utilities;
using System.Collections;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal abstract class AllMembersCollection<T> : IMemberCollection<T>
    where T : IMember
{
    private volatile Dictionary<T, T>? _members;

    protected AllMembersCollection( INamedType declaringType )
    {
        this.DeclaringType = declaringType;
    }

    public INamedType DeclaringType { get; }

    public IEnumerable<T> OfName( string name ) => this.GetItemsCore( name ).Keys;

    public IEnumerator<T> GetEnumerator() => this.GetItems().Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    protected abstract IMemberCollection<T> GetMembers( INamedType namedType );

    public int Count => this.GetItems().Count;

    private Dictionary<T, T> GetItemsCore( string? name )
    {
        // We don't assign the field directly so we don't get into concurrent updates of the collection.
        var members = new Dictionary<T, T>( MemberComparer<T>.Instance );

        for ( var t = this.DeclaringType; t != null; t = t.DeclaringType )
        {
            var includePrivate = t == this.DeclaringType;
            var declaredMembers = name == null ? this.GetMembers( t ) : this.GetMembers( t ).OfName( name );

            foreach ( var member in declaredMembers )
            {
                if ( !includePrivate && member.Accessibility == Accessibility.Private )
                {
                    continue;
                }

                if ( !members.ContainsKey( member ) )
                {
                    members.Add( member, member );
                }
            }
        }

        return members;
    }

    private Dictionary<T, T> GetItems()
    {
        if ( this._members == null )
        {
            this._members = this.GetItemsCore( null );
        }

        return this._members;
    }
}