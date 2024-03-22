// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;

// ReSharper disable NotAccessedPositionalProperty.Global

namespace Metalama.Framework.Engine.Linking;

internal sealed record InliningContextIdentifier( IntermediateSymbolSemantic<IMethodSymbol> DestinationSemantic, int? InliningId = null );