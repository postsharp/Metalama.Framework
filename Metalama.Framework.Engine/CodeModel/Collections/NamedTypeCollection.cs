// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal sealed class NamedTypeCollection : MemberOrNamedTypeCollection<INamedType>, INamedTypeCollection
    {
        public NamedTypeCollection( INamedType declaringType, UpdatableMemberCollection<INamedType> sourceItems ) :
            base( declaringType, sourceItems ) { }

        public NamedTypeCollection( ICompilation declaringCompilation, UpdatableMemberCollection<INamedType> sourceItems ) :
            base( declaringCompilation, sourceItems ) { }

        public NamedTypeCollection( INamespace declaringNamespace, UpdatableMemberCollection<INamedType> sourceItems ) :
            base( declaringNamespace, sourceItems ) { }

        public IEnumerable<INamedType> OfTypeDefinition( INamedType typeDefinition )
        {
            var typedSource = (INamedTypeCollectionImpl) this.Source;

            // Enumerate the source without causing a resolution of the reference.
            foreach ( var sourceItem in typedSource.OfTypeDefinition( typeDefinition ) )
            {
                // Resolve the reference and store the declaration.
                var member = this.GetItem( sourceItem.ToRef() );

                // Return the result.
                yield return member;
            }
        }
    }
}