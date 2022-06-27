// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal class AllOperatorsCollection : AllMembersCollection<IMethod>, IMethodCollection
{
    public AllOperatorsCollection( INamedType declaringType ) : base( declaringType ) { }

    protected override IMemberCollection<IMethod> GetMembers( INamedType namedType ) => namedType.Operators;

    public IEnumerable<IMethod> OfCompatibleSignature( string name, IReadOnlyList<Type?>? argumentTypes, bool? isStatic )
        => throw new NotImplementedException();

    public IEnumerable<IMethod> OfCompatibleSignature(
        string name,
        IReadOnlyList<IType?>? argumentTypes,
        IReadOnlyList<RefKind?>? refKinds = null,
        bool? isStatic = null )
        => throw new NotImplementedException();

    public IMethod? OfExactSignature( string name, IReadOnlyList<IType> parameterTypes, IReadOnlyList<RefKind>? refKinds = null, bool? isStatic = null )
        => throw new NotImplementedException();

    public IMethod? OfExactSignature( IMethod signatureTemplate, bool matchIsStatic = true ) => throw new NotImplementedException();

    public IEnumerable<IMethod> OfKind( MethodKind kind ) => this.Where( m => m.MethodKind == kind );
}