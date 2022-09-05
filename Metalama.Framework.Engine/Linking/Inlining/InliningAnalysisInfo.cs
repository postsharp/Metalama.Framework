// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Linking.Inlining
{
    public class InliningAnalysisInfo
    {
        public StatementSyntax ReplacedStatement { get; }

        public string? ReturnVariableIdentifier { get; }

        public InliningAnalysisInfo(StatementSyntax replacedStatement, string? returnVariableIdentifier )
        {
            this.ReplacedStatement = replacedStatement;
            this.ReturnVariableIdentifier = returnVariableIdentifier;
        }   
    }
}