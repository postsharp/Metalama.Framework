// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.References;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal abstract class MemberList<TMember, TSource> : DeclarationList<TMember, TSource>, IMemberList<TMember>
        where TMember : class, IMember
        where TSource : IMemberRef<TMember>
    {
        protected MemberList( IDeclaration? containingDeclaration, IEnumerable<TSource> sourceItems ) : base( containingDeclaration, sourceItems ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberList{TCodeElement, TSource}"/> class that represents an empty list.
        /// </summary>
        protected MemberList() { }

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