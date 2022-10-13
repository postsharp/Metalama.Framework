// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

// ReSharper disable InconsistentlySynchronizedField

namespace Metalama.Framework.DesignTime.Pipeline
{
    internal class MetalamaProjectClassifier : IMetalamaProjectClassifier
    {
        public bool IsMetalamaEnabled( Compilation compilation )
            => compilation.SyntaxTrees.FirstOrDefault()?.Options.PreprocessorSymbolNames.Contains( "METALAMA" ) ?? false;
    }
}