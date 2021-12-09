// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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