// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Introspection;

public interface IIntrospectionReferenceGraph
{
    IEnumerable<IIntrospectionDeclarationReference> GetInboundReferences(
        IDeclaration destination,
        IntrospectionChildKinds childKinds = IntrospectionChildKinds.ContainingDeclaration,
        CancellationToken cancellationToken = default );

    IEnumerable<IIntrospectionDeclarationReference> GetOutboundReferences( IDeclaration origin, CancellationToken cancellationToken = default );
}