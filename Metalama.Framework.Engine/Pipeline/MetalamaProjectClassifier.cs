// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Linq;

// ReSharper disable InconsistentlySynchronizedField

namespace Metalama.Framework.Engine.Pipeline;

internal class MetalamaProjectClassifier : IMetalamaProjectClassifier
{
    public bool IsMetalamaEnabled( Compilation compilation )
    {
        if ( compilation.SyntaxTrees.FirstOrDefault()?.Options.PreprocessorSymbolNames.Contains( "METALAMA" ) != true )
        {
            return false;
        }

        if ( !compilation.SourceModule.ReferencedAssemblies
                .Any( identity => identity.Name == "Metalama.Framework" ) )
        {
            return false;
        }

        return true;
    }
}