// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Generic;

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Result of member analysis.
    /// </summary>
    internal readonly struct MethodBodyAnalysisResult
    {
        public IReadOnlyList<ResolvedAspectReference> AspectReferences { get; }

        public MethodBodyAnalysisResult( IReadOnlyList<ResolvedAspectReference> aspectReferences )
        {
            this.AspectReferences = aspectReferences;
        }
    }
}