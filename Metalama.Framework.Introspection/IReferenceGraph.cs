// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Introspection;

public interface IReferenceGraph
{
    IReadOnlyCollection<IDeclarationReference> GetInboundReferences(
        IDeclaration destination,
        ReferenceGraphChildKinds childKinds = ReferenceGraphChildKinds.ContainingDeclaration );
}