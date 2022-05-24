// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal class EventCollection : MemberCollection<IEvent>, IEventCollection
    {
        public EventCollection( INamedType declaringType, EventUpdatableCollection sourceItems ) : base( declaringType, sourceItems ) { }

        public IEvent? OfExactSignature( IEvent signatureTemplate, bool matchIsStatic = true )
        {
            foreach ( var candidate in this.OfName( signatureTemplate.Name ) )
            {
                if ( matchIsStatic && candidate.IsStatic != signatureTemplate.IsStatic )
                {
                    continue;
                }

                return candidate;
            }

            return null;
        }
    }
}