// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal class AllMethodsCollection : AllMembersCollection<IMethod>, IMethodCollection
{
    public AllMethodsCollection( INamedType declaringType ) : base( declaringType ) { }

    protected override IMemberCollection<IMethod> GetMembers( INamedType namedType ) => namedType.Methods;

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

    public IEnumerable<IMethod> OfKind( OperatorKind kind ) => this.Where( m => m.OperatorKind == kind );
}