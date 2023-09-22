// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Options;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.HierarchicalOptions;

internal class SdkHierarchicalOptionsManager : ISdkHierarchicalOptionsManager
{
    private readonly CompilationModel _compilationModel;

    public SdkHierarchicalOptionsManager( CompilationModel compilationModel )
    {
        this._compilationModel = compilationModel;
    }

    public IHierarchicalOptions GetOptions( ISymbol symbol, Type optionsType )
    {
        var declaration = this._compilationModel.Factory.GetDeclaration( symbol );

        return this._compilationModel.HierarchicalOptionsManager.GetOptions( declaration, optionsType );
    }
}