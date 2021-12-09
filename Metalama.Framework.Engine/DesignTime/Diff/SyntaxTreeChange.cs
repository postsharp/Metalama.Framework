// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.DesignTime.Diff
{
    /// <summary>
    /// Represents a change between two versions of a <see cref="SyntaxTree"/>.
    /// </summary>
    internal readonly struct SyntaxTreeChange
    {
        /// <summary>
        /// Gets the kind of change between the old and new syntax trees.
        /// </summary>
        public SyntaxTreeChangeKind SyntaxTreeChangeKind { get; }

        /// <summary>
        /// Gets a value indicating whether the new syntax tree contain compile-time code.
        /// </summary>
        public bool HasCompileTimeCode { get; }

        /// <summary>
        /// Gets a value indicating how the <see cref="HasCompileTimeCode"/> value has changed between the old
        /// and the new syntax tree.
        /// </summary>
        public CompileTimeChangeKind CompileTimeChangeKind { get; }

        /// <summary>
        /// Gets the path of the syntax tree.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Gets the new syntax tree, unless the current item represents a deleted tree.
        /// </summary>
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

        public override string ToString() => $"{this.FilePath}, ChangeKind={this.SyntaxTreeChangeKind}, CompileTimeChangeKind={this.CompileTimeChangeKind}";

        public SyntaxTreeChange Merge( SyntaxTreeChange newChange )
        {
            var newSyntaxTreeChangeKind = (this.SyntaxTreeChangeKind, newChange.SyntaxTreeChangeKind) switch
            {
                (SyntaxTreeChangeKind.Added, SyntaxTreeChangeKind.Changed) => SyntaxTreeChangeKind.Added,
                (SyntaxTreeChangeKind.Added, SyntaxTreeChangeKind.Deleted) => SyntaxTreeChangeKind.None,
                (SyntaxTreeChangeKind.Added, SyntaxTreeChangeKind.Added) => throw new AssertionFailedException(),
                (_, SyntaxTreeChangeKind.Deleted) => SyntaxTreeChangeKind.Deleted,
                (SyntaxTreeChangeKind.Deleted, SyntaxTreeChangeKind.Added) => SyntaxTreeChangeKind.Changed,
                (SyntaxTreeChangeKind.Deleted, _) => throw new AssertionFailedException(),
                (SyntaxTreeChangeKind.Changed, SyntaxTreeChangeKind.Changed) => SyntaxTreeChangeKind.Changed,
                _ => throw new AssertionFailedException()
            };

            var newCompileTimeChangeKind = (this.CompileTimeChangeKind, newChange.CompileTimeChangeKind) switch
            {
                (_, CompileTimeChangeKind.None) => this.CompileTimeChangeKind,
                (CompileTimeChangeKind.None, _) => newChange.CompileTimeChangeKind,
                (CompileTimeChangeKind.NewlyCompileTime, CompileTimeChangeKind.NoLongerCompileTime) => CompileTimeChangeKind.None,
                (CompileTimeChangeKind.NewlyCompileTime, _) => CompileTimeChangeKind.NewlyCompileTime,
                (CompileTimeChangeKind.NoLongerCompileTime, CompileTimeChangeKind.NewlyCompileTime) => CompileTimeChangeKind.None,

                _ => throw new AssertionFailedException()
            };

            return new SyntaxTreeChange( this.FilePath, newSyntaxTreeChangeKind, newChange.HasCompileTimeCode, newCompileTimeChangeKind, newChange.NewTree );
        }
    }
}