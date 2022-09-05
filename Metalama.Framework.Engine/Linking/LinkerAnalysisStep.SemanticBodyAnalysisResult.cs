// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerAnalysisStep
    {
        private class SemanticBodyAnalysisResult
        {
            /// <summary>
            /// Gets properties of return statements.
            /// </summary>
            public IReadOnlyDictionary<ReturnStatementSyntax, ReturnStatementProperties> ReturnStatements { get; }

            /// <summary>
            /// Gets a value indicating whether the end-point of the body is reachable. This can be true only for void-returning methods.
            /// </summary>
            public bool HasReachableEndPoint { get; }

            public SemanticBodyAnalysisResult( IReadOnlyDictionary<ReturnStatementSyntax, ReturnStatementProperties> returnStatements, bool hasReachableEndPoint )
            {
                this.ReturnStatements = returnStatements;
                this.HasReachableEndPoint = hasReachableEndPoint;
            }
        }
    }
}