// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Impl.CodeModel.References;
using System.Collections.Generic;

namespace Metalama.Framework.Impl.CodeModel.Collections
{
    internal abstract class MemberOrNamedTypeList<TMember, TSource> : DeclarationList<TMember, TSource>, IMemberList<TMember>
        where TMember : class, IMemberOrNamedType
        where TSource : IMemberRef<TMember>
    {
        protected MemberOrNamedTypeList( IDeclaration? containingDeclaration, IEnumerable<TSource> sourceItems ) :
            base( containingDeclaration, sourceItems ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberOrNamedTypeList{TMember,TSource}"/> class that represents an empty list.
        /// </summary>
        protected MemberOrNamedTypeList() { }

        public IEnumerable<TMember> OfName( string name )
        {
            for ( var i = 0; i < this.Count; i++ )
            {
                var sourceItem = this.SourceItems[i];

                if ( sourceItem.Name == name )
                {
                    yield return this[i];
                }
            }
        }
    }
}