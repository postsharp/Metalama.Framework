// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Result of member analysis.
    /// </summary>
    internal class MemberAnalysisResult
    {
        public bool HasSimpleReturnControlFlow { get; }

        public MemberAnalysisResult( bool hasSimpleReturnControlFlow )
        {
            this.HasSimpleReturnControlFlow = hasSimpleReturnControlFlow;
        }
    }
}