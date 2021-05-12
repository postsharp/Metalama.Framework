// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.DesignTime
{
    internal readonly struct SyntaxTreeChange
    {
        public SyntaxTreeChangeKind SyntaxTreeChangeKind { get; }

        public bool HasCompileTimeCode { get; }

        public CompileTimeChangeKind CompileTimeChangeKind { get; }

        public string FilePath { get; }

        public SyntaxTree? NewTree { get; }

        public SyntaxTreeChange(
            string filePath,
            SyntaxTreeChangeKind syntaxTreeChangeKind,
            bool hasCompileTimeCode,
            CompileTimeChangeKind compileTimeChangeKind,
            SyntaxTree? newTree )
        {
            this.SyntaxTreeChangeKind = syntaxTreeChangeKind;
            this.HasCompileTimeCode = hasCompileTimeCode;
            this.CompileTimeChangeKind = compileTimeChangeKind;
            this.FilePath = filePath;
            this.NewTree = newTree;
        }
    }
}