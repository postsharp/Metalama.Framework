// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Metalama.Framework.DesignTime.Contracts.AspectExplorer;

[Guid( "AF977B33-AA8C-4481-9B7E-B14A67200429" )]
public struct AspectExplorerAspectInstance
{
    public ISymbol TargetDeclaration;
    public AspectExplorerAspectTransformation[] Transformations;
}

[Guid( "E758C91B-E335-4D53-AA30-82BBCCBF428A" )]
public struct AspectExplorerAspectTransformation
{
    public ISymbol TargetDeclaration;
    public string Description;
}