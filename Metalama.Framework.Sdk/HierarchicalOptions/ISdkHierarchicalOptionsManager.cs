// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Options;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.HierarchicalOptions;

internal interface ISdkHierarchicalOptionsManager
{
    IHierarchicalOptions GetOptions( ISymbol symbol, Type optionsType );
}