// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerAnalysisStep
    {
        private class ReturnStatementProperties
        {
            /// <summary>
            /// Gets a value indicating whether the control would flow to the method exit if the return statement was replaced.
            /// </summary>
            public bool FlowsToExitIfRewritten { get; }

            public ReturnStatementProperties( bool flowsToExitIfRewritten )
            {
                this.FlowsToExitIfRewritten = flowsToExitIfRewritten;
            }
        }
    }
}