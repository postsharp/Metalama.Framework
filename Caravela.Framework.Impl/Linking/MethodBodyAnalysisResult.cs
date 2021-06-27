// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Result of member analysis.
    /// </summary>
    internal readonly struct MethodBodyAnalysisResult
    {
        public bool HasSimpleReturnControlFlow { get; }

        public IReadOnlyList<AspectReferenceHandle> AspectReferences { get; }

        public MethodBodyAnalysisResult( IReadOnlyList<AspectReferenceHandle> aspectReferences, bool hasSimpleReturnControlFlow )
        {
            this.AspectReferences = aspectReferences;
            this.HasSimpleReturnControlFlow = hasSimpleReturnControlFlow;
        }
    }
}