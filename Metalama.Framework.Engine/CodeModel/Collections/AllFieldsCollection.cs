// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal class AllFieldsCollection : AllMembersCollection<IField>, IFieldCollection
{
    public AllFieldsCollection( NamedType declaringType ) : base( declaringType ) { }

    protected override IMemberCollection<IField> GetMembers( INamedType namedType ) => namedType.Fields;
}