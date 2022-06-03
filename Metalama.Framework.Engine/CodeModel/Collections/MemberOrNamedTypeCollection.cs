// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal abstract class MemberOrNamedTypeCollection<TMember> : DeclarationCollection<TMember, Ref<TMember>>, IMemberOrNamedTypeCollection<TMember>
    where TMember : class, IMemberOrNamedType
{
    protected MemberOrNamedTypeCollection( IDeclaration containingDeclaration, UpdatableMemberCollection<TMember> sourceItems ) :
        base( containingDeclaration, sourceItems ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberOrNamedTypeCollection{TMember}"/> class that represents an empty list.
    /// </summary>
    protected MemberOrNamedTypeCollection() { }

    public IEnumerable<TMember> OfName( string name )
    {
        var typedSource = (UpdatableMemberCollection<TMember>) this.Source;

        // Enumerate the source without causing a resolution of the reference.
        foreach ( var sourceItem in typedSource.OfName( name ) )
        {
            // Resolve the reference and store the declaration.
            var member = this.GetItem( sourceItem.ToRef() );

            // Return the result.
            yield return member;
        }
    }
}