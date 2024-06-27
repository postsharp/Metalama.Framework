// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Introspection;

public interface IReferenceGraph
{
    IEnumerable<IDeclarationReference> GetInboundReferences(
        IDeclaration destination,
        ReferenceGraphChildKinds childKinds = ReferenceGraphChildKinds.ContainingDeclaration,
        CancellationToken cancellationToken = default );

    IEnumerable<IDeclarationReference> GetOutboundReferences( IDeclaration origin, CancellationToken cancellationToken = default );
}