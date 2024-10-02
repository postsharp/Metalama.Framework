﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal sealed class AllTypesCollection : AllMemberOrNamedTypesCollection<INamedType, INamedTypeCollection>, INamedTypeCollection
{
    public AllTypesCollection( INamedTypeImpl declaringType ) : base( declaringType ) { }

    protected override IEqualityComparer<INamedType> Comparer => this.CompilationContext.Comparers.GetTypeComparer( Code.Comparers.TypeComparison.Default );

    public IEnumerable<INamedType> OfTypeDefinition( INamedType typeDefinition )
    {
        return this.Where( p => p.Is( typeDefinition, ConversionKind.TypeDefinition ) );
    }

    protected override INamedTypeCollection GetMembers( INamedType namedType ) => namedType.Types;
}