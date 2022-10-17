﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal class AllFieldsCollection : AllMembersCollection<IField>, IFieldCollection
{
    public AllFieldsCollection( NamedType declaringType ) : base( declaringType ) { }

    protected override IMemberCollection<IField> GetMembers( INamedType namedType ) => namedType.Fields;

    public IField this[ string name ] => this.OfName( name ).Single();
}