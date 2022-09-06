// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Pipeline
{
    public sealed class IntroducedSyntaxTree
    {
        public string Name { get; }

        public SyntaxTree SourceSyntaxTree { get; }

        public SyntaxTree GeneratedSyntaxTree { get; }

        public IntroducedSyntaxTree( string name, SyntaxTree sourceSyntaxTree, SyntaxTree generatedSyntaxTree )
        {
            this.Name = name;
            this.SourceSyntaxTree = sourceSyntaxTree;
            this.GeneratedSyntaxTree = generatedSyntaxTree;
        }
    }
}