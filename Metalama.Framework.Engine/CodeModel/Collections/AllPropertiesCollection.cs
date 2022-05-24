// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal class AllPropertiesCollection : AllMembersCollection<IProperty>, IPropertyCollection
{
    public AllPropertiesCollection( INamedType declaringType ) : base( declaringType ) { }

    protected override IMemberCollection<IProperty> GetMembers( INamedType namedType ) => namedType.Properties;
}