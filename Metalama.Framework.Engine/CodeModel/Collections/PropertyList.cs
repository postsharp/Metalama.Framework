// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal class PropertyList : MemberOrNamedTypeList<IProperty, MemberRef<IProperty>>, IPropertyList
    {
        public PropertyList( NamedType containingDeclaration, IEnumerable<MemberRef<IProperty>> sourceItems ) : base( containingDeclaration, sourceItems ) { }

        public IProperty? OfExactSignature( IProperty signatureTemplate, bool matchIsStatic = true, bool declaredOnly = true )
        {
            // TODO: This is temporary.
            foreach ( var candidate in this.OfName( signatureTemplate.Name ) )
            {
                if ( matchIsStatic && candidate.IsStatic != signatureTemplate.IsStatic )
                {
                    continue;
                }

                return candidate;
            }

            if ( declaredOnly || this.ContainingDeclaration is not NamedType namedType || namedType.BaseType == null )
            {
                return null;
            }
            else
            {
                return namedType.BaseType.Properties.OfExactSignature( signatureTemplate, matchIsStatic, declaredOnly );
            }
        }
    }
}