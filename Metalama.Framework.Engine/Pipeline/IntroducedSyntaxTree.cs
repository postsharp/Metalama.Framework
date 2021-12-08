// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Impl.Pipeline
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