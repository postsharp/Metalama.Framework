// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.References;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal abstract class MemberList<TCodeElement, TSource> : DeclarationList<TCodeElement, TSource>, IMemberList<TCodeElement>
        where TCodeElement : class, IMember
        where TSource : IMemberRef<TCodeElement>
    {
        protected MemberList( IDeclaration? containingElement, IEnumerable<TSource> sourceItems ) : base( containingElement, sourceItems ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberList{TCodeElement, TSource}"/> class that represents an empty list.
        /// </summary>
        protected MemberList() { }

        public IEnumerable<TCodeElement> OfName( string name )
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