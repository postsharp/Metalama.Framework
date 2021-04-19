// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal abstract class MemberList<TCodeElement, TSource> : CodeElementList<TCodeElement, TSource>, IMemberList<TCodeElement>
        where TCodeElement : class, IMember
        where TSource : IMemberLink<TCodeElement>
    {
        protected MemberList( ICodeElement? containingElement, IEnumerable<TSource> sourceItems ) : base( containingElement, sourceItems )
        {
        }

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