// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal sealed class PropertyCollection : MemberCollection<IProperty>, IPropertyCollection
    {
        public PropertyCollection( INamedType declaringType, PropertyUpdatableCollection sourceItems ) : base( declaringType, sourceItems ) { }

        public IProperty this[ string name ] => this.OfName( name ).Single();
    }
}