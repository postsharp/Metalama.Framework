// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Linking
{
    internal record InliningContextIdentifier(IntermediateSymbolSemantic<IMethodSymbol> DestinationSemantic, int? InliningId = null)
    {
    }
}