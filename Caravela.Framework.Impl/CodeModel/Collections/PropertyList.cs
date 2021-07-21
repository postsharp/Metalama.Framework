// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class PropertyList : MemberOrNamedTypeList<IProperty, MemberRef<IProperty>>, IPropertyList
    {
        public PropertyList( NamedType containingDeclaration, IEnumerable<MemberRef<IProperty>> sourceItems ) : base( containingDeclaration, sourceItems ) { }

        public IProperty? OfExactSignature( IProperty signatureTemplate, bool matchIsStatic = true, bool declaredOnly = true )
        {
            // TODO: This should use at least some code from the base class (specifically for indexers).
            if (signatureTemplate.Parameters.Count > 0)
            {
                throw new NotImplementedException();
            }

            // TODO: This is temporary.
            foreach (var candidate in this.OfName( signatureTemplate.Name ))
            {
                if (matchIsStatic && candidate.IsStatic != signatureTemplate.IsStatic)
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