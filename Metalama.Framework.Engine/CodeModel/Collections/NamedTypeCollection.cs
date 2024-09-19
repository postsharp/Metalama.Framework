// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal sealed class NamedTypeCollection : MemberOrNamedTypeCollection<INamedType>, INamedTypeCollection
    {
        public NamedTypeCollection( INamedType declaringType, ITypeUpdatableCollection sourceItems ) :
            base( declaringType, sourceItems ) { }

        public NamedTypeCollection( ICompilation declaringCompilation, ITypeUpdatableCollection sourceItems ) :
            base( declaringCompilation, sourceItems ) { }

        public NamedTypeCollection( INamespace declaringNamespace, ITypeUpdatableCollection sourceItems ) :
            base( declaringNamespace, sourceItems ) { }

        public IEnumerable<INamedType> OfTypeDefinition( INamedType typeDefinition )
        {
            var typedSource = (ITypeUpdatableCollection) this.Source;

            // Enumerate the source without causing a resolution of the reference.
            foreach ( var sourceItem in typedSource.OfTypeDefinition( typeDefinition ) )
            {
                // Resolve the reference and store the declaration.
                var member = this.GetItem( sourceItem );

                // Return the result.
                yield return member;
            }
        }
    }
}